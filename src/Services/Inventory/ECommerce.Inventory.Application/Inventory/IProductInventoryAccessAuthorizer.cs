namespace ECommerce.Inventory.Application.Inventory;

public interface IProductInventoryAccessAuthorizer
{
    Task<ProductInventoryAccessResult> AuthorizeAsync(
        Guid productId,
        string? accessToken,
        CancellationToken cancellationToken);
}
