namespace ECommerce.Catalog.Application.Products;

public interface IProductReferenceReader
{
    Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken cancellationToken);

    Task<bool> BrandExistsAsync(Guid brandId, CancellationToken cancellationToken);
}
