using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Catalog.Application.Products;

public interface IProductStoreAccessValidator
{
    Task<Result> ValidateAsync(
        Guid? storeId,
        ProductAccessContext access,
        bool requireStoreForSeller,
        CancellationToken cancellationToken);
}
