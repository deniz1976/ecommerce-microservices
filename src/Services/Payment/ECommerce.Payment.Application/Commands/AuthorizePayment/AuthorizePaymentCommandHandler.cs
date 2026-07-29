using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.Payment.Application.Payments;
using ECommerce.Payment.Domain;

namespace ECommerce.Payment.Application.Commands.AuthorizePayment;

public sealed class AuthorizePaymentCommandHandler
    : ICommandHandler<AuthorizePaymentCommand, PaymentAuthorizationResult>
{
    private readonly IRepository<Domain.Payment, Guid> repository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IPaymentIdentityReader identityReader;
    private readonly IPaymentProvider paymentProvider;

    public AuthorizePaymentCommandHandler(
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

    public async Task<PaymentAuthorizationResult> HandleAsync(
        AuthorizePaymentCommand command,
        CancellationToken cancellationToken)
    {
        PaymentAuthorizationRequest request = command.Request;
        Guid? existingPaymentId = await identityReader.FindIdByOrderIdAsync(
            request.OrderId,
            cancellationToken);
        if (existingPaymentId is not null)
        {
            Domain.Payment existingPayment = (await repository.GetByIdAsync(
                existingPaymentId.Value,
                cancellationToken))!;
            return existingPayment.Status == PaymentStatus.Failed
                ? new PaymentAuthorizationResult(
                    false,
                    existingPayment.Id,
                    ErrorCodes.PaymentFailed,
                    existingPayment.FailureReason ?? "Payment failed.")
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
                paymentProvider.Name,
                "Payment amount and currency must be valid.");

            repository.Add(failedPayment);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return new PaymentAuthorizationResult(
                false,
                failedPayment.Id,
                ErrorCodes.PaymentFailed,
                failedPayment.FailureReason);
        }

        PaymentProviderAuthorizationResult providerResult = await paymentProvider.AuthorizeAsync(
            new PaymentProviderAuthorizationRequest(
                request.OrderId,
                request.CustomerId,
                request.Amount,
                normalizedCurrency),
            cancellationToken);

        Domain.Payment payment = CreatePayment(request, normalizedCurrency, providerResult);
        repository.Add(payment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return providerResult.Succeeded
            ? new PaymentAuthorizationResult(true, payment.Id, null, null)
            : new PaymentAuthorizationResult(
                false,
                payment.Id,
                ErrorCodes.PaymentFailed,
                payment.FailureReason);
    }

    private Domain.Payment CreatePayment(
        PaymentAuthorizationRequest request,
        string normalizedCurrency,
        PaymentProviderAuthorizationResult providerResult)
    {
        if (!providerResult.Succeeded)
        {
            return Domain.Payment.CreateFailed(
                request.OrderId,
                request.CustomerId,
                request.Amount,
                normalizedCurrency,
                paymentProvider.Name,
                "Payment authorization was declined.");
        }

        if (string.IsNullOrWhiteSpace(providerResult.PaymentReference) ||
            string.IsNullOrWhiteSpace(providerResult.TransactionReference))
        {
            throw new PaymentProviderOperationException(
                "The payment provider returned an incomplete authorization result.");
        }

        return Domain.Payment.CreateAuthorized(
            request.OrderId,
            request.CustomerId,
            request.Amount,
            normalizedCurrency,
            paymentProvider.Name,
            providerResult.PaymentReference,
            providerResult.TransactionReference);
    }
}
