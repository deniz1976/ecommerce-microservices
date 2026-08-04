using ECommerce.Payment.Domain;

namespace ECommerce.Payment.Application.Payments;

public sealed record PaymentSummaryResponse(
    Guid Id,
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string Currency,
    PaymentStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
