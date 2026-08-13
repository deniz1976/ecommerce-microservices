using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.References;

public sealed class CatalogBrandManagementService(
    IRepository<Brand, Guid> brandRepository,
    IUnitOfWork unitOfWork,
    ICatalogReferenceConflictReader conflictReader)
{
    public async Task<Result<CatalogBrandResponse>> CreateAsync(
        CreateCatalogBrandRequest request,
        CancellationToken cancellationToken)
    {
        string name = request.Name.Trim();
        string slug = CatalogReferenceInput.NormalizeSlug(request.Slug);
        if (!CatalogReferenceInput.IsValidName(name) ||
            !CatalogReferenceInput.IsValidSlug(slug))
        {
            return CatalogReferenceInput.ValidationFailure<CatalogBrandResponse>();
        }

        if (await conflictReader.BrandSlugExistsAsync(slug, null, cancellationToken))
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

    public async Task<Result<ManagedCatalogBrandResponse>> UpdateAsync(
        Guid brandId,
        UpdateCatalogBrandRequest request,
        CancellationToken cancellationToken)
    {
        string name = request.Name.Trim();
        string slug = CatalogReferenceInput.NormalizeSlug(request.Slug);
        if (brandId == Guid.Empty ||
            !CatalogReferenceInput.IsValidName(name) ||
            !CatalogReferenceInput.IsValidSlug(slug))
        {
            return CatalogReferenceInput.ValidationFailure<ManagedCatalogBrandResponse>();
        }

        Brand? brand = await brandRepository.GetByIdAsync(brandId, cancellationToken);
        if (brand is null)
        {
            return Result<ManagedCatalogBrandResponse>.Failure(new Error(
                CatalogErrorCodes.BrandNotFound,
                CatalogErrorCodes.BrandNotFound));
        }

        if (await conflictReader.BrandSlugExistsAsync(slug, brandId, cancellationToken))
        {
            return Result<ManagedCatalogBrandResponse>.Failure(new Error(
                CatalogErrorCodes.BrandSlugConflict,
                CatalogErrorCodes.BrandSlugConflict));
        }

        brand.Update(name, slug, request.IsActive);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ManagedCatalogBrandResponse>.Success(
            new ManagedCatalogBrandResponse(brand.Id, brand.Name, brand.Slug, brand.IsActive));
    }
}
