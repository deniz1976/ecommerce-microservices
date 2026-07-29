namespace ECommerce.Payment.Application.Payments;

public sealed record PaymentProviderAuthorizationRequest(
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string Currency);
