using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.Payment.Domain;

namespace ECommerce.Payment.Application.Payments;

public sealed class PaymentRefundService
{
    private readonly IRepository<Domain.Payment, Guid> repository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IPaymentIdentityReader identityReader;
    private readonly IPaymentProvider paymentProvider;

    public PaymentRefundService(
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

    public async Task RefundAsync(
        RefundPaymentRequest request,
        CancellationToken cancellationToken)
    {
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
        string normalizedCurrency = request.Currency.Trim().ToUpperInvariant();
        PaymentRefundEligibility eligibility = payment.EvaluateRefund(
            request.CustomerId,
            request.Amount,
            normalizedCurrency);
        if (eligibility is PaymentRefundEligibility.AlreadyRefunded or
            PaymentRefundEligibility.InvalidStatus)
        {
            return;
        }

        if (eligibility != PaymentRefundEligibility.Eligible)
        {
            throw new PaymentRefundIntegrityException(
                $"Refund request does not match the persisted payment ({eligibility}).");
        }

        if (string.IsNullOrWhiteSpace(payment.ProviderPaymentReference))
        {
            throw new PaymentProviderOperationException(
                "The payment does not contain a provider reference required for refund.");
        }

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

        PaymentRefundEligibility mutationResult = payment.MarkRefunded(
            request.CustomerId,
            request.Amount,
            normalizedCurrency,
            providerResult.TransactionReference,
            request.Reason);
        if (mutationResult != PaymentRefundEligibility.Eligible)
        {
            throw new PaymentRefundIntegrityException(
                $"Payment became ineligible for refund ({mutationResult}).");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
