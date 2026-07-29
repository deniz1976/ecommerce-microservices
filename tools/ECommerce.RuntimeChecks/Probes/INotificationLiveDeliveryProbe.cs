using ECommerce.RuntimeChecks.Models;

namespace ECommerce.RuntimeChecks.Probes;

internal interface INotificationLiveDeliveryProbe
{
    Task<RuntimeNotificationMessage> CaptureOrderNotificationAsync(
        Guid customerId,
        Func<CancellationToken, Task<OrderResponse>> trigger,
        DateTimeOffset deadline,
        CancellationToken cancellationToken);
}
