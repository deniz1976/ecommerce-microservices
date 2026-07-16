using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Images;
using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.Products;

public sealed class ProductService
{
    private static readonly HashSet<string> SupportedLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        "en",
        "tr"
    };

    private readonly IProductRepository repository;
    private readonly ICloudImageService cloudImageService;

    public ProductService(IProductRepository repository, ICloudImageService cloudImageService)
    {
        this.repository = repository;
        this.cloudImageService = cloudImageService;
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

    public async Task<Result<ProductResponse>> CreateAsync(CreateProductRequest request, string culture, CancellationToken cancellationToken)
    {
        Result validation = await ValidateReferencesAndTranslationsAsync(request.CategoryId, request.BrandId, request.Translations, cancellationToken);

        if (validation.IsFailure)
        {
            return Result<ProductResponse>.Failure(validation.Error!);
        }

        Product product = new(Guid.NewGuid(), request.Sku, request.CategoryId, request.BrandId, request.Price, request.Currency, request.Status);

        foreach (ProductTranslationInput translation in request.Translations)
        {
            product.SetTranslation(translation.LanguageCode.ToLowerInvariant(), translation.Name, translation.Description);
        }

        foreach (ProductImageInput image in request.Images)
        {
            bool imageExists = await cloudImageService.ImageExistsAsync(image.PublicId, cancellationToken);

            if (!imageExists)
            {
                return Result<ProductResponse>.Failure(new Error(ErrorCodes.ValidationFailed, ErrorCodes.ValidationFailed));
            }

            product.AddImage(new ProductImage(
                Guid.NewGuid(),
                product.Id,
                image.PublicId,
                image.Url,
                image.SecureUrl,
                image.Width,
                image.Height,
                image.Format,
                image.SortOrder,
                image.IsMain));
        }

        repository.Add(product);
        await repository.SaveChangesAsync(cancellationToken);

        Product? createdProduct = await repository.GetByIdAsync(product.Id, cancellationToken);

        return Result<ProductResponse>.Success((createdProduct ?? product).ToResponse(culture));
    }

    public async Task<Result<ProductResponse>> UpdateAsync(Guid id, UpdateProductRequest request, string culture, CancellationToken cancellationToken)
    {
        Product? product = await repository.GetByIdAsync(id, cancellationToken);

        if (product is null)
        {
            return Result<ProductResponse>.Failure(new Error(ErrorCodes.ProductNotFound, ErrorCodes.ProductNotFound));
        }

        Result validation = await ValidateReferencesAndTranslationsAsync(request.CategoryId, request.BrandId, request.Translations, cancellationToken);

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

    private async Task<Result> ValidateReferencesAndTranslationsAsync(
        Guid categoryId,
        Guid brandId,
        IReadOnlyCollection<ProductTranslationInput> translations,
        CancellationToken cancellationToken)
    {
        if (!await repository.CategoryExistsAsync(categoryId, cancellationToken))
        {
            return Result.Failure(new Error(CatalogErrorCodes.CategoryNotFound, CatalogErrorCodes.CategoryNotFound));
        }

        if (!await repository.BrandExistsAsync(brandId, cancellationToken))
        {
            return Result.Failure(new Error(CatalogErrorCodes.BrandNotFound, CatalogErrorCodes.BrandNotFound));
        }

        bool hasEnglish = translations.Any(x => x.LanguageCode.Equals("en", StringComparison.OrdinalIgnoreCase));
        bool allLanguagesSupported = translations.All(x => SupportedLanguages.Contains(x.LanguageCode));

        if (!hasEnglish || !allLanguagesSupported)
        {
            return Result.Failure(new Error(CatalogErrorCodes.InvalidProductTranslation, CatalogErrorCodes.InvalidProductTranslation));
        }

        return Result.Success();
    }
}
