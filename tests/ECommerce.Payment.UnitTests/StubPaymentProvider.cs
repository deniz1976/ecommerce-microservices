using ECommerce.Payment.Application.Payments;

namespace ECommerce.Payment.UnitTests;

internal sealed class StubPaymentProvider : IPaymentProvider
{
    public string Name => "StubProvider";

    public PaymentProviderAuthorizationResult AuthorizationResult { get; set; } =
        new(true, "payment-reference", "authorization-reference", null);

    public PaymentProviderRefundResult RefundResult { get; set; } =
        new(true, "refund-reference", null);

    public int AuthorizationCount { get; private set; }

    public int RefundCount { get; private set; }

    public Task<PaymentProviderAuthorizationResult> AuthorizeAsync(
        PaymentProviderAuthorizationRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        AuthorizationCount++;
        return Task.FromResult(AuthorizationResult);
    }

    public Task<PaymentProviderRefundResult> RefundAsync(
        PaymentProviderRefundRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RefundCount++;
        return Task.FromResult(RefundResult);
    }
}
