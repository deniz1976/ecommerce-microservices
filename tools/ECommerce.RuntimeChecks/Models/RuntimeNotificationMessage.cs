namespace ECommerce.RuntimeChecks.Models;

internal sealed record RuntimeNotificationMessage(
    Guid Id,
    Guid CustomerId,
    Guid? OrderId,
    string Type,
    string Title,
    string Message,
    string Culture,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt);
