using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.Payment.Application.Commands.AuthorizePayment;
using ECommerce.Payment.Application.Commands.RefundPayment;
using ECommerce.Payment.Application.Payments;
using ECommerce.Payment.Domain;

namespace ECommerce.Payment.UnitTests;

public sealed class PaymentCommandHandlerTests
{
    [Fact]
    public async Task AuthorizePersistsProviderReferences()
    {
        InMemoryPaymentRepository repository = new();
        StubPaymentProvider provider = new();
        AuthorizePaymentCommandHandler handler = new(
            new PaymentService(repository, repository, repository, provider));

        PaymentAuthorizationResult result = await handler.HandleAsync(
            new AuthorizePaymentCommand(
                new PaymentAuthorizationRequest(Guid.NewGuid(), Guid.NewGuid(), 25.50m, "try")),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(1, provider.AuthorizationCount);
        Assert.Equal("StubProvider", repository.Payment!.ProviderName);
        Assert.Equal("payment-reference", repository.Payment.ProviderPaymentReference);
        Assert.Contains(
            repository.Payment.Transactions,
            transaction => transaction.ProviderTransactionReference == "authorization-reference");
    }

    [Fact]
    public async Task DuplicateAuthorizationDoesNotCallProviderAgain()
    {
        Guid orderId = Guid.NewGuid();
        InMemoryPaymentRepository repository = new();
        StubPaymentProvider provider = new();
        AuthorizePaymentCommandHandler handler = new(
            new PaymentService(repository, repository, repository, provider));
        PaymentAuthorizationRequest request = new(orderId, Guid.NewGuid(), 12m, "USD");

        PaymentAuthorizationResult first = await handler.HandleAsync(
            new AuthorizePaymentCommand(request),
            CancellationToken.None);
        PaymentAuthorizationResult duplicate = await handler.HandleAsync(
            new AuthorizePaymentCommand(request),
            CancellationToken.None);

        Assert.True(first.Succeeded);
        Assert.True(duplicate.Succeeded);
        Assert.Equal(first.PaymentId, duplicate.PaymentId);
        Assert.Equal(1, provider.AuthorizationCount);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task ProviderDeclinePersistsCustomerSafeFailure()
    {
        InMemoryPaymentRepository repository = new();
        StubPaymentProvider provider = new()
        {
            AuthorizationResult = new(false, null, null, "Raw provider decline detail")
        };
        AuthorizePaymentCommandHandler handler = new(
            new PaymentService(repository, repository, repository, provider));

        PaymentAuthorizationResult result = await handler.HandleAsync(
            new AuthorizePaymentCommand(
                new PaymentAuthorizationRequest(Guid.NewGuid(), Guid.NewGuid(), 8m, "USD")),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCodes.PaymentFailed, result.ReasonCode);
        Assert.Equal("Payment authorization was declined.", result.Reason);
        Assert.DoesNotContain("Raw provider", repository.Payment!.FailureReason);
    }

    [Fact]
    public async Task RefundPersistsProviderTransactionReference()
    {
        Guid orderId = Guid.NewGuid();
        InMemoryPaymentRepository repository = new();
        StubPaymentProvider provider = new();
        PaymentService paymentService = new(repository, repository, repository, provider);
        AuthorizePaymentCommandHandler authorizeHandler = new(paymentService);
        RefundPaymentCommandHandler refundHandler = new(paymentService);
        await authorizeHandler.HandleAsync(
            new AuthorizePaymentCommand(
                new PaymentAuthorizationRequest(orderId, Guid.NewGuid(), 19m, "USD")),
            CancellationToken.None);

        await refundHandler.HandleAsync(
            new RefundPaymentCommand(
                new RefundPaymentRequest(
                    orderId,
                    repository.Payment!.CustomerId,
                    19m,
                    "usd",
                    "Shipment failed.")),
            CancellationToken.None);

        Assert.Equal(1, provider.RefundCount);
        Assert.Equal(PaymentStatus.Refunded, repository.Payment.Status);
        Assert.Contains(
            repository.Payment.Transactions,
            transaction =>
                transaction.Type == PaymentTransactionType.Refund &&
                transaction.ProviderTransactionReference == "refund-reference");
    }

    [Fact]
    public async Task RefundFailureLeavesPaymentAuthorizedAndSignalsRetry()
    {
        Guid orderId = Guid.NewGuid();
        InMemoryPaymentRepository repository = new();
        StubPaymentProvider provider = new()
        {
            RefundResult = new(false, null, "Raw demo refund failure")
        };
        PaymentService paymentService = new(repository, repository, repository, provider);
        AuthorizePaymentCommandHandler authorizeHandler = new(paymentService);
        RefundPaymentCommandHandler refundHandler = new(paymentService);
        await authorizeHandler.HandleAsync(
            new AuthorizePaymentCommand(
                new PaymentAuthorizationRequest(
                    orderId,
                    Guid.NewGuid(),
                    19m,
                    "USD")),
            CancellationToken.None);

        await Assert.ThrowsAsync<PaymentProviderOperationException>(
            () => refundHandler.HandleAsync(
                new RefundPaymentCommand(
                    new RefundPaymentRequest(
                        orderId,
                        repository.Payment!.CustomerId,
                        19m,
                        "USD",
                        "Shipment failed.")),
                CancellationToken.None));

        Assert.Equal(1, provider.RefundCount);
        Assert.Equal(PaymentStatus.Authorized, repository.Payment!.Status);
        Assert.Equal(1, repository.SaveCount);
        Assert.DoesNotContain(
            repository.Payment.Transactions,
            transaction => transaction.Type == PaymentTransactionType.Refund);
    }
}
