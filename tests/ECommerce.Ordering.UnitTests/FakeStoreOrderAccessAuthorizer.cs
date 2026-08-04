using ECommerce.Ordering.Application.Orders;

namespace ECommerce.Ordering.UnitTests;

internal sealed class FakeStoreOrderAccessAuthorizer(
    StoreOrderAccessResult result = StoreOrderAccessResult.Granted)
    : IStoreOrderAccessAuthorizer
{
    public Guid? ReceivedStoreId { get; private set; }

    public string? ReceivedAccessToken { get; private set; }

    public Task<StoreOrderAccessResult> AuthorizeAsync(
        Guid storeId,
        string? accessToken,
        CancellationToken cancellationToken)
    {
        ReceivedStoreId = storeId;
        ReceivedAccessToken = accessToken;
        return Task.FromResult(result);
    }
}
