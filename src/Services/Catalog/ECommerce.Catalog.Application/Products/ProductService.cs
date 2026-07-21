using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.Products;

public sealed class ProductService
{
    private readonly IProductRepository repository;
    private readonly IProductStoreAccessValidator storeAccessValidator;
    private readonly IProductReferenceValidator referenceValidator;
    private readonly IProductImageAttacher imageAttacher;

    public ProductService(
        IProductRepository repository,
        IProductStoreAccessValidator storeAccessValidator,
        IProductReferenceValidator referenceValidator,
        IProductImageAttacher imageAttacher)
    {
        this.repository = repository;
        this.storeAccessValidator = storeAccessValidator;
        this.referenceValidator = referenceValidator;
        this.imageAttacher = imageAttacher;
    }

    public async Task<Result<PagedResult<ProductResponse>>> SearchAsync(ProductListQuery query, string culture, CancellationToken cancellationToken)
    {
        PagedResult<Product> products = await repository.SearchAsync(query, cancellationToken);
        PagedResult<ProductResponse> response = new(
            products.Items.Select(product => product.ToResponse(culture)).ToArray(),
            products.PageNumber,
            products.PageSize,
            products.TotalCount);

        return Result<PagedResult<ProductResponse>>.Success(response);
    }

    public async Task<Result<ProductResponse>> GetByIdAsync(Guid id, string culture, CancellationToken cancellationToken)
    {
        Product? product = await repository.GetByIdAsync(id, cancellationToken);

        return product is null
            ? Result<ProductResponse>.Failure(new Error(ErrorCodes.ProductNotFound, ErrorCodes.ProductNotFound))
            : Result<ProductResponse>.Success(product.ToResponse(culture));
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
            product.SetTranslation(translation.LanguageCode.ToLowerInvariant(), translation.Name, translation.Description);
        }

        Result imageResult = await imageAttacher.AttachAsync(product, request.Images, cancellationToken);
        if (imageResult.IsFailure)
        {
            return Result<ProductResponse>.Failure(imageResult.Error!);
        }

        repository.Add(product);
        await repository.SaveChangesAsync(cancellationToken);

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
            return Result<ProductResponse>.Failure(new Error(ErrorCodes.ProductNotFound, ErrorCodes.ProductNotFound));
        }

        if (!access.IsAdmin && product.StoreId is null)
        {
            return Result<ProductResponse>.Failure(
                new Error(CatalogErrorCodes.StoreAccessDenied, CatalogErrorCodes.StoreAccessDenied));
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

        product.UpdateDetails(request.CategoryId, request.BrandId, request.Price, request.Currency, request.Status);

        foreach (ProductTranslationInput translation in request.Translations)
        {
            product.SetTranslation(translation.LanguageCode.ToLowerInvariant(), translation.Name, translation.Description);
        }

        await repository.SaveChangesAsync(cancellationToken);

        return Result<ProductResponse>.Success(product.ToResponse(culture));
    }

}
