using ECommerce.Payment.Application.Payments;
using Microsoft.Extensions.Options;

namespace ECommerce.Payment.Infrastructure.Payments;

public sealed class DemoPaymentProvider : IPaymentProvider
{
    private readonly DemoPaymentOptions options;

    public DemoPaymentProvider(IOptions<DemoPaymentOptions> options)
    {
        this.options = options.Value;
    }

    public string Name => "Demo";

    public Task<PaymentProviderAuthorizationResult> AuthorizeAsync(
        PaymentProviderAuthorizationRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (options.Scenario == DemoPaymentScenario.TransientFailure)
        {
            throw new PaymentProviderOperationException(
                "The demo payment provider simulated a transient authorization failure.");
        }

        if (options.Scenario == DemoPaymentScenario.Decline)
        {
            return Task.FromResult(
                new PaymentProviderAuthorizationResult(
                    false,
                    null,
                    null,
                    "The demo payment provider simulated a decline."));
        }

        string orderReference = request.OrderId.ToString("N");
        return Task.FromResult(
            new PaymentProviderAuthorizationResult(
                true,
                $"demo_payment_{orderReference}",
                $"demo_authorization_{orderReference}",
                null));
    }

    public Task<PaymentProviderRefundResult> RefundAsync(
        PaymentProviderRefundRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (options.Scenario == DemoPaymentScenario.TransientFailure ||
            options.Scenario == DemoPaymentScenario.RefundFailure)
        {
            return Task.FromResult(
                new PaymentProviderRefundResult(
                    false,
                    null,
                    "The demo payment provider simulated a refund failure."));
        }

        return Task.FromResult(
            new PaymentProviderRefundResult(
                true,
                $"demo_refund_{request.OrderId:N}",
                null));
    }
}
