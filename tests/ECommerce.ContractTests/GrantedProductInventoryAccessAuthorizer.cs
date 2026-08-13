using ECommerce.Inventory.Application.Inventory;

namespace ECommerce.ContractTests;

internal sealed class GrantedProductInventoryAccessAuthorizer : IProductInventoryAccessAuthorizer
{
    public Task<ProductInventoryAccessResult> AuthorizeAsync(
        Guid productId,
        string? accessToken,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(ProductInventoryAccessResult.Granted);
    }
}
