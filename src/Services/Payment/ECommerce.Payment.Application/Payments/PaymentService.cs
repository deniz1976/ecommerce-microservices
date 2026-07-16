using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.Payment.Domain;

namespace ECommerce.Payment.Application.Payments;

public sealed class PaymentService
{
    private readonly IPaymentRepository paymentRepository;

    public PaymentService(IPaymentRepository paymentRepository)
    {
        this.paymentRepository = paymentRepository;
    }

    public async Task<PaymentAuthorizationResult> AuthorizeAsync(PaymentAuthorizationRequest request, CancellationToken cancellationToken)
    {
        Domain.Payment? existingPayment = await paymentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        if (existingPayment is not null)
        {
            return existingPayment.Status == PaymentStatus.Failed
                ? new PaymentAuthorizationResult(false, existingPayment.Id, ErrorCodes.PaymentFailed, existingPayment.FailureReason ?? "Payment failed.")
                : new PaymentAuthorizationResult(true, existingPayment.Id, null, null);
        }

        string normalizedCurrency = request.Currency.Trim().ToUpperInvariant();
        if (request.Amount <= 0 || string.IsNullOrWhiteSpace(normalizedCurrency))
        {
            Domain.Payment failedPayment = Domain.Payment.CreateFailed(
                request.OrderId,
                request.CustomerId,
                request.Amount,
                normalizedCurrency,
                "Payment amount and currency must be valid.");

            paymentRepository.Add(failedPayment);
            await paymentRepository.SaveChangesAsync(cancellationToken);

            return new PaymentAuthorizationResult(false, failedPayment.Id, ErrorCodes.PaymentFailed, failedPayment.FailureReason);
        }

        Domain.Payment authorizedPayment = Domain.Payment.CreateAuthorized(
            request.OrderId,
            request.CustomerId,
            request.Amount,
            normalizedCurrency);

        paymentRepository.Add(authorizedPayment);
        await paymentRepository.SaveChangesAsync(cancellationToken);

        return new PaymentAuthorizationResult(true, authorizedPayment.Id, null, null);
    }

    public async Task RefundAsync(RefundPaymentRequest request, CancellationToken cancellationToken)
    {
        Domain.Payment? payment = await paymentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        if (payment is null || payment.Status == PaymentStatus.Failed)
        {
            return;
        }

        payment.MarkRefunded(request.Amount, request.Currency.Trim().ToUpperInvariant(), request.Reason);
        await paymentRepository.SaveChangesAsync(cancellationToken);
    }
}
