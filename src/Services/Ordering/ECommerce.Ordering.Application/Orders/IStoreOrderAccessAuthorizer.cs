namespace ECommerce.Ordering.Application.Orders;

public interface IStoreOrderAccessAuthorizer
{
    Task<StoreOrderAccessResult> AuthorizeAsync(
        Guid storeId,
        string? accessToken,
        CancellationToken cancellationToken);
}
