using System.Text.RegularExpressions;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.References;

public sealed partial class CatalogReferenceManagementService
{
    private readonly IRepository<Category, Guid> categoryRepository;
    private readonly IRepository<Brand, Guid> brandRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly ICatalogReferenceConflictReader conflictReader;

    public CatalogReferenceManagementService(
        IRepository<Category, Guid> categoryRepository,
        IRepository<Brand, Guid> brandRepository,
        IUnitOfWork unitOfWork,
        ICatalogReferenceConflictReader conflictReader)
    {
        this.categoryRepository = categoryRepository;
        this.brandRepository = brandRepository;
        this.unitOfWork = unitOfWork;
        this.conflictReader = conflictReader;
    }

    public async Task<Result<CatalogCategoryResponse>> CreateCategoryAsync(
        CreateCatalogCategoryRequest request,
        string culture,
        CancellationToken cancellationToken)
    {
        string slug = NormalizeSlug(request.Slug);
        string englishName = request.EnglishName.Trim();
        string turkishName = request.TurkishName.Trim();
        if (!IsValidSlug(slug) ||
            !IsValidName(englishName) ||
            !IsValidName(turkishName))
        {
            return ValidationFailure<CatalogCategoryResponse>();
        }

        if (await conflictReader.CategorySlugExistsAsync(
            slug,
            excludedCategoryId: null,
            cancellationToken))
        {
            return Result<CatalogCategoryResponse>.Failure(new Error(
                CatalogErrorCodes.CategorySlugConflict,
                CatalogErrorCodes.CategorySlugConflict));
        }

        Category category = new(Guid.NewGuid(), slug, isActive: true);
        category.SetTranslation("en", englishName);
        category.SetTranslation("tr", turkishName);
        categoryRepository.Add(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        string name = culture.Equals("tr", StringComparison.OrdinalIgnoreCase)
            ? turkishName
            : englishName;
        return Result<CatalogCategoryResponse>.Success(
            new CatalogCategoryResponse(category.Id, name, category.Slug));
    }

    public async Task<Result<CatalogBrandResponse>> CreateBrandAsync(
        CreateCatalogBrandRequest request,
        CancellationToken cancellationToken)
    {
        string name = request.Name.Trim();
        string slug = NormalizeSlug(request.Slug);
        if (!IsValidName(name) || !IsValidSlug(slug))
        {
            return ValidationFailure<CatalogBrandResponse>();
        }

        if (await conflictReader.BrandSlugExistsAsync(
            slug,
            excludedBrandId: null,
            cancellationToken))
        {
            return Result<CatalogBrandResponse>.Failure(new Error(
                CatalogErrorCodes.BrandSlugConflict,
                CatalogErrorCodes.BrandSlugConflict));
        }

        Brand brand = new(Guid.NewGuid(), name, slug, isActive: true);
        brandRepository.Add(brand);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<CatalogBrandResponse>.Success(
            new CatalogBrandResponse(brand.Id, brand.Name, brand.Slug));
    }

    public async Task<Result<ManagedCatalogCategoryResponse>> UpdateCategoryAsync(
        Guid categoryId,
        UpdateCatalogCategoryRequest request,
        CancellationToken cancellationToken)
    {
        string slug = NormalizeSlug(request.Slug);
        string englishName = request.EnglishName.Trim();
        string turkishName = request.TurkishName.Trim();
        if (categoryId == Guid.Empty ||
            !IsValidSlug(slug) ||
            !IsValidName(englishName) ||
            !IsValidName(turkishName))
        {
            return ValidationFailure<ManagedCatalogCategoryResponse>();
        }

        Category? category = await categoryRepository.GetByIdAsync(
            categoryId,
            cancellationToken);
        if (category is null)
        {
            return Result<ManagedCatalogCategoryResponse>.Failure(new Error(
                CatalogErrorCodes.CategoryNotFound,
                CatalogErrorCodes.CategoryNotFound));
        }

        if (await conflictReader.CategorySlugExistsAsync(
            slug,
            categoryId,
            cancellationToken))
        {
            return Result<ManagedCatalogCategoryResponse>.Failure(new Error(
                CatalogErrorCodes.CategorySlugConflict,
                CatalogErrorCodes.CategorySlugConflict));
        }

        category.Update(slug, request.IsActive);
        category.SetTranslation("en", englishName);
        category.SetTranslation("tr", turkishName);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ManagedCatalogCategoryResponse>.Success(
            ToManagedResponse(category));
    }

    public async Task<Result<ManagedCatalogBrandResponse>> UpdateBrandAsync(
        Guid brandId,
        UpdateCatalogBrandRequest request,
        CancellationToken cancellationToken)
    {
        string name = request.Name.Trim();
        string slug = NormalizeSlug(request.Slug);
        if (brandId == Guid.Empty ||
            !IsValidName(name) ||
            !IsValidSlug(slug))
        {
            return ValidationFailure<ManagedCatalogBrandResponse>();
        }

        Brand? brand = await brandRepository.GetByIdAsync(
            brandId,
            cancellationToken);
        if (brand is null)
        {
            return Result<ManagedCatalogBrandResponse>.Failure(new Error(
                CatalogErrorCodes.BrandNotFound,
                CatalogErrorCodes.BrandNotFound));
        }

        if (await conflictReader.BrandSlugExistsAsync(
            slug,
            brandId,
            cancellationToken))
        {
            return Result<ManagedCatalogBrandResponse>.Failure(new Error(
                CatalogErrorCodes.BrandSlugConflict,
                CatalogErrorCodes.BrandSlugConflict));
        }

        brand.Update(name, slug, request.IsActive);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ManagedCatalogBrandResponse>.Success(
            new ManagedCatalogBrandResponse(
                brand.Id,
                brand.Name,
                brand.Slug,
                brand.IsActive));
    }

    private static Result<T> ValidationFailure<T>() =>
        Result<T>.Failure(new Error(
            ErrorCodes.ValidationFailed,
            ErrorCodes.ValidationFailed));

    private static string NormalizeSlug(string value) =>
        value.Trim().ToLowerInvariant();

    private static bool IsValidSlug(string value) =>
        value.Length is >= 2 and <= 160 && SlugRegex().IsMatch(value);

    private static bool IsValidName(string value) =>
        value.Length is >= 2 and <= 256;

    private static ManagedCatalogCategoryResponse ToManagedResponse(
        Category category)
    {
        string englishName = category.Translations
            .FirstOrDefault(item => item.LanguageCode == "en")?.Name
            ?? category.Slug;
        string turkishName = category.Translations
            .FirstOrDefault(item => item.LanguageCode == "tr")?.Name
            ?? englishName;
        return new ManagedCatalogCategoryResponse(
            category.Id,
            englishName,
            turkishName,
            category.Slug,
            category.IsActive);
    }

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex SlugRegex();
}
