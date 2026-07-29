namespace ECommerce.Payment.Application.Payments;

public sealed record PaymentProviderRefundRequest(
    Guid OrderId,
    string PaymentReference,
    decimal Amount,
    string Currency,
    string Reason);
