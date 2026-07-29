namespace ECommerce.Payment.Application.Payments;

public sealed record PaymentProviderAuthorizationResult(
    bool Succeeded,
    string? PaymentReference,
    string? TransactionReference,
    string? FailureReason);
