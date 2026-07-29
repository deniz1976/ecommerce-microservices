namespace ECommerce.RuntimeChecks.Models;

internal sealed record RuntimeCheckoutBasketResponse(
    Guid SnapshotId,
    Guid CustomerId,
    decimal TotalAmount,
    string Currency,
    DateTimeOffset CreatedAt);
