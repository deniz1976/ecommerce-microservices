namespace ECommerce.Payment.Application.Payments;

public interface IPaymentProvider
{
    string Name { get; }

    Task<PaymentProviderAuthorizationResult> AuthorizeAsync(
        PaymentProviderAuthorizationRequest request,
        CancellationToken cancellationToken);

    Task<PaymentProviderRefundResult> RefundAsync(
        PaymentProviderRefundRequest request,
        CancellationToken cancellationToken);
}
