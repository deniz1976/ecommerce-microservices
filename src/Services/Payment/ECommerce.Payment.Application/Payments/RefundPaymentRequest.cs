namespace ECommerce.Payment.Application.Payments;

public sealed record RefundPaymentRequest(
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string Currency,
    string Reason);
