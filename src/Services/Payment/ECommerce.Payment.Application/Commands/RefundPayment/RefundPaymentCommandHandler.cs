using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.Payment.Application.Payments;
using ECommerce.Payment.Domain;

namespace ECommerce.Payment.Application.Commands.RefundPayment;

public sealed class RefundPaymentCommandHandler : ICommandHandler<RefundPaymentCommand>
{
    private readonly IRepository<Domain.Payment, Guid> repository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IPaymentIdentityReader identityReader;
    private readonly IPaymentProvider paymentProvider;

    public RefundPaymentCommandHandler(
        IRepository<Domain.Payment, Guid> repository,
        IUnitOfWork unitOfWork,
        IPaymentIdentityReader identityReader,
        IPaymentProvider paymentProvider)
    {
        this.repository = repository;
        this.unitOfWork = unitOfWork;
        this.identityReader = identityReader;
        this.paymentProvider = paymentProvider;
    }

    public async Task HandleAsync(
        RefundPaymentCommand command,
        CancellationToken cancellationToken)
    {
        RefundPaymentRequest request = command.Request;
        Guid? paymentId = await identityReader.FindIdByOrderIdAsync(
            request.OrderId,
            cancellationToken);
        if (paymentId is null)
        {
            return;
        }

        Domain.Payment payment = (await repository.GetByIdAsync(
            paymentId.Value,
            cancellationToken))!;
        if (payment.Status is PaymentStatus.Failed or PaymentStatus.Refunded)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(payment.ProviderPaymentReference))
        {
            throw new PaymentProviderOperationException(
                "The payment does not contain a provider reference required for refund.");
        }

        string normalizedCurrency = request.Currency.Trim().ToUpperInvariant();
        PaymentProviderRefundResult providerResult = await paymentProvider.RefundAsync(
            new PaymentProviderRefundRequest(
                request.OrderId,
                payment.ProviderPaymentReference,
                request.Amount,
                normalizedCurrency,
                request.Reason),
            cancellationToken);

        if (!providerResult.Succeeded ||
            string.IsNullOrWhiteSpace(providerResult.TransactionReference))
        {
            throw new PaymentProviderOperationException(
                "The payment provider could not complete the refund.");
        }

        payment.MarkRefunded(
            request.Amount,
            normalizedCurrency,
            providerResult.TransactionReference,
            request.Reason);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
