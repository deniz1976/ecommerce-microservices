namespace ECommerce.RuntimeChecks.Models;

internal sealed record RuntimeOrderStatusHistoryResponse(
    RuntimeOrderStatus Status,
    DateTimeOffset OccurredAt,
    string? ReasonCode);
