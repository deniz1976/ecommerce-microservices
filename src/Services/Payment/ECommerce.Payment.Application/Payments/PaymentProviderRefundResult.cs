namespace ECommerce.Payment.Application.Payments;

public sealed record PaymentProviderRefundResult(
    bool Succeeded,
    string? TransactionReference,
    string? FailureReason);
