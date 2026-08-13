using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.Products;

public sealed class ProductManagementService
{
    private readonly IRepository<Product, Guid> repository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IProductStoreAccessValidator storeAccessValidator;
    private readonly IProductReferenceValidator referenceValidator;

    public ProductManagementService(
        IRepository<Product, Guid> repository,
        IUnitOfWork unitOfWork,
        IProductStoreAccessValidator storeAccessValidator,
        IProductReferenceValidator referenceValidator)
    {
        this.repository = repository;
        this.unitOfWork = unitOfWork;
        this.storeAccessValidator = storeAccessValidator;
        this.referenceValidator = referenceValidator;
    }

    public async Task<Result<ProductResponse>> CreateAsync(
        CreateProductRequest request,
        ProductAccessContext access,
        string culture,
        CancellationToken cancellationToken)
    {
        Result storeAccess = await storeAccessValidator.ValidateAsync(
            request.StoreId,
            access,
            requireStoreForSeller: true,
            cancellationToken);
        if (storeAccess.IsFailure)
        {
            return Result<ProductResponse>.Failure(storeAccess.Error!);
        }

        Result validation = await referenceValidator.ValidateAsync(
            request.CategoryId,
            request.BrandId,
            request.Translations,
            cancellationToken);
        if (validation.IsFailure)
        {
            return Result<ProductResponse>.Failure(validation.Error!);
        }

        Product product = new(
            Guid.NewGuid(),
            request.Sku,
            request.CategoryId,
            request.BrandId,
            request.StoreId,
            request.Price,
            request.Currency,
            request.Status);

        foreach (ProductTranslationInput translation in request.Translations)
        {
            product.SetTranslation(
                translation.LanguageCode.ToLowerInvariant(),
                translation.Name,
                translation.Description);
        }

        repository.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        Product? createdProduct = await repository.GetByIdAsync(product.Id, cancellationToken);
        return Result<ProductResponse>.Success((createdProduct ?? product).ToResponse(culture));
    }

    public async Task<Result<ProductResponse>> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        ProductAccessContext access,
        string culture,
        CancellationToken cancellationToken)
    {
        Product? product = await repository.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return Result<ProductResponse>.Failure(
                new Error(ErrorCodes.ProductNotFound, ErrorCodes.ProductNotFound));
        }

        if (!access.IsAdmin && product.StoreId is null)
        {
            return Result<ProductResponse>.Failure(
                new Error(
                    CatalogErrorCodes.StoreAccessDenied,
                    CatalogErrorCodes.StoreAccessDenied));
        }

        Result storeAccess = await storeAccessValidator.ValidateAsync(
            product.StoreId,
            access,
            requireStoreForSeller: true,
            cancellationToken);
        if (storeAccess.IsFailure)
        {
            return Result<ProductResponse>.Failure(storeAccess.Error!);
        }

        Result validation = await referenceValidator.ValidateAsync(
            request.CategoryId,
            request.BrandId,
            request.Translations,
            cancellationToken);
        if (validation.IsFailure)
        {
            return Result<ProductResponse>.Failure(validation.Error!);
        }

        product.UpdateDetails(
            request.CategoryId,
            request.BrandId,
            request.Price,
            request.Currency,
            request.Status);

        foreach (ProductTranslationInput translation in request.Translations)
        {
            product.SetTranslation(
                translation.LanguageCode.ToLowerInvariant(),
                translation.Name,
                translation.Description);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ProductResponse>.Success(product.ToResponse(culture));
    }
}
