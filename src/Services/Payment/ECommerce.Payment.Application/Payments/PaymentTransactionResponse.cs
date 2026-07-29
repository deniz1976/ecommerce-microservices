using ECommerce.Payment.Domain;

namespace ECommerce.Payment.Application.Payments;

public sealed record PaymentTransactionResponse(
    Guid Id,
    PaymentTransactionType Type,
    decimal Amount,
    string Currency,
    DateTimeOffset CreatedAt);
