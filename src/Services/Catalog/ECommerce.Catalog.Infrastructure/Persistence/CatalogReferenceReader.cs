using ECommerce.Catalog.Application.References;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Catalog.Infrastructure.Persistence;

public sealed class CatalogReferenceReader :
    ICatalogReferenceReader,
    ICatalogReferenceConflictReader,
    ICatalogReferenceManagementReader
{
    private readonly CatalogDbContext dbContext;

    public CatalogReferenceReader(CatalogDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .Include(category => category.Translations)
            .Where(category => category.IsActive)
            .OrderBy(category => category.Slug)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Brand>> GetActiveBrandsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Brands
            .AsNoTracking()
            .Where(brand => brand.IsActive)
            .OrderBy(brand => brand.Name)
            .ToArrayAsync(cancellationToken);
    }

    public Task<bool> CategorySlugExistsAsync(
        string slug,
        Guid? excludedCategoryId,
        CancellationToken cancellationToken)
    {
        return dbContext.Categories.AnyAsync(
            category =>
                category.Slug == slug &&
                (!excludedCategoryId.HasValue ||
                 category.Id != excludedCategoryId.Value),
            cancellationToken);
    }

    public Task<bool> BrandSlugExistsAsync(
        string slug,
        Guid? excludedBrandId,
        CancellationToken cancellationToken)
    {
        return dbContext.Brands.AnyAsync(
            brand =>
                brand.Slug == slug &&
                (!excludedBrandId.HasValue ||
                 brand.Id != excludedBrandId.Value),
            cancellationToken);
    }

    public async Task<PagedResult<ManagedCatalogCategoryResponse>> GetCategoriesAsync(
        ManagedCatalogCategoryListCriteria criteria,
        CancellationToken cancellationToken)
    {
        int pageSize = NormalizePageSize(criteria.PageSize);
        IQueryable<Category> categories = dbContext.Categories.AsNoTracking();

        if (criteria.IsActive.HasValue)
        {
            categories = categories.Where(category =>
                category.IsActive == criteria.IsActive.Value);
        }

        string? searchPattern = CreateSearchPattern(criteria.Search);
        if (searchPattern is not null)
        {
            categories = categories.Where(category =>
                EF.Functions.ILike(category.Slug, searchPattern, "\\") ||
                category.Translations.Any(translation =>
                    EF.Functions.ILike(translation.Name, searchPattern, "\\")));
        }

        long totalCount = await categories.LongCountAsync(cancellationToken);
        int pageNumber = NormalizePageNumber(criteria.PageNumber, pageSize, totalCount);

        var rows =
            from category in categories
            join englishTranslation in dbContext.CategoryTranslations
                    .Where(translation => translation.LanguageCode == "en")
                on category.Id equals englishTranslation.CategoryId into englishTranslations
            from english in englishTranslations.DefaultIfEmpty()
            join turkishTranslation in dbContext.CategoryTranslations
                    .Where(translation => translation.LanguageCode == "tr")
                on category.Id equals turkishTranslation.CategoryId into turkishTranslations
            from turkish in turkishTranslations.DefaultIfEmpty()
            select new
            {
                category.Id,
                EnglishName = english.Name ?? category.Slug,
                TurkishName = turkish.Name ?? english.Name ?? category.Slug,
                category.Slug,
                category.IsActive,
            };

        bool descending = criteria.SortDescending;
        var orderedRows = criteria.SortBy?.Trim() switch
        {
            CatalogReferenceSortFields.TurkishName => descending
                ? rows.OrderByDescending(row => row.TurkishName).ThenBy(row => row.Id)
                : rows.OrderBy(row => row.TurkishName).ThenBy(row => row.Id),
            CatalogReferenceSortFields.Slug => descending
                ? rows.OrderByDescending(row => row.Slug).ThenBy(row => row.Id)
                : rows.OrderBy(row => row.Slug).ThenBy(row => row.Id),
            CatalogReferenceSortFields.IsActive => descending
                ? rows.OrderByDescending(row => row.IsActive).ThenBy(row => row.Id)
                : rows.OrderBy(row => row.IsActive).ThenBy(row => row.Id),
            _ => descending
                ? rows.OrderByDescending(row => row.EnglishName).ThenBy(row => row.Id)
                : rows.OrderBy(row => row.EnglishName).ThenBy(row => row.Id),
        };

        ManagedCatalogCategoryResponse[] items = await orderedRows
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(row => new ManagedCatalogCategoryResponse(
                row.Id,
                row.EnglishName,
                row.TurkishName,
                row.Slug,
                row.IsActive))
            .ToArrayAsync(cancellationToken);

        return new PagedResult<ManagedCatalogCategoryResponse>(
            items,
            pageNumber,
            pageSize,
            totalCount);
    }

    public async Task<PagedResult<ManagedCatalogBrandResponse>> GetBrandsAsync(
        ManagedCatalogBrandListCriteria criteria,
        CancellationToken cancellationToken)
    {
        int pageSize = NormalizePageSize(criteria.PageSize);
        IQueryable<Brand> brands = dbContext.Brands.AsNoTracking();

        if (criteria.IsActive.HasValue)
        {
            brands = brands.Where(brand => brand.IsActive == criteria.IsActive.Value);
        }

        string? searchPattern = CreateSearchPattern(criteria.Search);
        if (searchPattern is not null)
        {
            brands = brands.Where(brand =>
                EF.Functions.ILike(brand.Name, searchPattern, "\\") ||
                EF.Functions.ILike(brand.Slug, searchPattern, "\\"));
        }

        long totalCount = await brands.LongCountAsync(cancellationToken);
        int pageNumber = NormalizePageNumber(criteria.PageNumber, pageSize, totalCount);

        brands = ApplyBrandOrdering(
            brands,
            criteria.SortBy,
            criteria.SortDescending);

        IQueryable<ManagedCatalogBrandResponse> projection = brands.Select(brand =>
            new ManagedCatalogBrandResponse(
                brand.Id,
                brand.Name,
                brand.Slug,
                brand.IsActive));

        ManagedCatalogBrandResponse[] items = await projection
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return new PagedResult<ManagedCatalogBrandResponse>(
            items,
            pageNumber,
            pageSize,
            totalCount);
    }

    private static IQueryable<Brand> ApplyBrandOrdering(
        IQueryable<Brand> query,
        string? sortBy,
        bool descending)
    {
        return sortBy?.Trim() switch
        {
            CatalogReferenceSortFields.Slug => descending
                ? query.OrderByDescending(brand => brand.Slug).ThenBy(brand => brand.Id)
                : query.OrderBy(brand => brand.Slug).ThenBy(brand => brand.Id),
            CatalogReferenceSortFields.IsActive => descending
                ? query.OrderByDescending(brand => brand.IsActive).ThenBy(brand => brand.Id)
                : query.OrderBy(brand => brand.IsActive).ThenBy(brand => brand.Id),
            _ => descending
                ? query.OrderByDescending(brand => brand.Name).ThenBy(brand => brand.Id)
                : query.OrderBy(brand => brand.Name).ThenBy(brand => brand.Id)
        };
    }

    private static int NormalizePageSize(int pageSize) =>
        pageSize is <= 0 or > CatalogReferenceQueryLimits.MaxPageSize
            ? CatalogReferenceQueryLimits.DefaultPageSize
            : pageSize;

    private static int NormalizePageNumber(
        int pageNumber,
        int pageSize,
        long totalCount)
    {
        long totalPages = Math.Max(1, (long)Math.Ceiling(totalCount / (double)pageSize));
        return (int)Math.Min(Math.Max(1, pageNumber), totalPages);
    }

    private static string? CreateSearchPattern(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return null;
        }

        string normalized = search.Trim();
        if (normalized.Length > CatalogReferenceQueryLimits.MaxSearchLength)
        {
            normalized = normalized[..CatalogReferenceQueryLimits.MaxSearchLength];
        }

        string escaped = normalized
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
        return $"%{escaped}%";
    }
}
