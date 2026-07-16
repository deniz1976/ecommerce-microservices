namespace ECommerce.Payment.Application.Payments;

public sealed record PaymentAuthorizationRequest(
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string Currency);
