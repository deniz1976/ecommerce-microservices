using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.References;

public sealed class CatalogCategoryManagementService(
    IRepository<Category, Guid> categoryRepository,
    IUnitOfWork unitOfWork,
    ICatalogReferenceConflictReader conflictReader)
{
    public async Task<Result<CatalogCategoryResponse>> CreateAsync(
        CreateCatalogCategoryRequest request,
        string culture,
        CancellationToken cancellationToken)
    {
        string slug = CatalogReferenceInput.NormalizeSlug(request.Slug);
        string englishName = request.EnglishName.Trim();
        string turkishName = request.TurkishName.Trim();
        if (!CatalogReferenceInput.IsValidSlug(slug) ||
            !CatalogReferenceInput.IsValidName(englishName) ||
            !CatalogReferenceInput.IsValidName(turkishName))
        {
            return CatalogReferenceInput.ValidationFailure<CatalogCategoryResponse>();
        }

        if (await conflictReader.CategorySlugExistsAsync(slug, null, cancellationToken))
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

    public async Task<Result<ManagedCatalogCategoryResponse>> UpdateAsync(
        Guid categoryId,
        UpdateCatalogCategoryRequest request,
        CancellationToken cancellationToken)
    {
        string slug = CatalogReferenceInput.NormalizeSlug(request.Slug);
        string englishName = request.EnglishName.Trim();
        string turkishName = request.TurkishName.Trim();
        if (categoryId == Guid.Empty ||
            !CatalogReferenceInput.IsValidSlug(slug) ||
            !CatalogReferenceInput.IsValidName(englishName) ||
            !CatalogReferenceInput.IsValidName(turkishName))
        {
            return CatalogReferenceInput.ValidationFailure<ManagedCatalogCategoryResponse>();
        }

        Category? category = await categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        if (category is null)
        {
            return Result<ManagedCatalogCategoryResponse>.Failure(new Error(
                CatalogErrorCodes.CategoryNotFound,
                CatalogErrorCodes.CategoryNotFound));
        }

        if (await conflictReader.CategorySlugExistsAsync(slug, categoryId, cancellationToken))
        {
            return Result<ManagedCatalogCategoryResponse>.Failure(new Error(
                CatalogErrorCodes.CategorySlugConflict,
                CatalogErrorCodes.CategorySlugConflict));
        }

        category.Update(slug, request.IsActive);
        category.SetTranslation("en", englishName);
        category.SetTranslation("tr", turkishName);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ManagedCatalogCategoryResponse>.Success(ToManagedResponse(category));
    }

    private static ManagedCatalogCategoryResponse ToManagedResponse(Category category)
    {
        string englishName = category.Translations
            .FirstOrDefault(item => item.LanguageCode == "en")?.Name ?? category.Slug;
        string turkishName = category.Translations
            .FirstOrDefault(item => item.LanguageCode == "tr")?.Name ?? englishName;
        return new ManagedCatalogCategoryResponse(
            category.Id,
            englishName,
            turkishName,
            category.Slug,
            category.IsActive);
    }
}
