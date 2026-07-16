namespace ECommerce.Payment.Application.Payments;

public sealed record PaymentAuthorizationResult(
    bool Succeeded,
    Guid? PaymentId,
    string? ReasonCode,
    string? Reason);
