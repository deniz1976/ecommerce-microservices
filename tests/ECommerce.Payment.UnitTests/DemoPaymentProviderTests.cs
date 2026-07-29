using ECommerce.Payment.Application.Payments;
using ECommerce.Payment.Infrastructure.Payments;
using Microsoft.Extensions.Options;

namespace ECommerce.Payment.UnitTests;

public sealed class DemoPaymentProviderTests
{
    [Fact]
    public async Task SuccessScenarioReturnsDeterministicReferences()
    {
        Guid orderId = Guid.NewGuid();
        DemoPaymentProvider provider = CreateProvider(DemoPaymentScenario.Success);

        PaymentProviderAuthorizationResult result = await provider.AuthorizeAsync(
            new PaymentProviderAuthorizationRequest(orderId, Guid.NewGuid(), 25m, "TRY"),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal($"demo_payment_{orderId:N}", result.PaymentReference);
        Assert.Equal($"demo_authorization_{orderId:N}", result.TransactionReference);
        Assert.Null(result.FailureReason);
    }

    [Fact]
    public async Task DeclineScenarioReturnsFailedAuthorizationWithoutReferences()
    {
        DemoPaymentProvider provider = CreateProvider(DemoPaymentScenario.Decline);

        PaymentProviderAuthorizationResult result = await provider.AuthorizeAsync(
            new PaymentProviderAuthorizationRequest(Guid.NewGuid(), Guid.NewGuid(), 25m, "TRY"),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Null(result.PaymentReference);
        Assert.Null(result.TransactionReference);
    }

    [Fact]
    public async Task TransientFailureScenarioThrowsForAuthorization()
    {
        DemoPaymentProvider provider = CreateProvider(DemoPaymentScenario.TransientFailure);

        await Assert.ThrowsAsync<PaymentProviderOperationException>(
            () => provider.AuthorizeAsync(
                new PaymentProviderAuthorizationRequest(Guid.NewGuid(), Guid.NewGuid(), 25m, "TRY"),
                CancellationToken.None));
    }

    [Fact]
    public async Task RefundFailureScenarioReturnsFailedRefund()
    {
        DemoPaymentProvider provider = CreateProvider(DemoPaymentScenario.RefundFailure);

        PaymentProviderRefundResult result = await provider.RefundAsync(
            new PaymentProviderRefundRequest(
                Guid.NewGuid(),
                "demo_payment_reference",
                25m,
                "TRY",
                "Shipment failed."),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Null(result.TransactionReference);
    }

    private static DemoPaymentProvider CreateProvider(DemoPaymentScenario scenario)
    {
        return new DemoPaymentProvider(
            Options.Create(new DemoPaymentOptions
            {
                Scenario = scenario
            }));
    }
}
