using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Payment.Application.Payments;

public sealed class PaymentQueryService
{
    private readonly IPaymentQueryReader paymentReader;

    public PaymentQueryService(IPaymentQueryReader paymentReader)
    {
        this.paymentReader = paymentReader;
    }

    public async Task<Result<PaymentResponse>> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        PaymentResponse? payment = await paymentReader.GetByOrderIdAsync(
            orderId,
            cancellationToken);
        return payment is null
            ? Result<PaymentResponse>.Failure(
                new Error(ErrorCodes.PaymentNotFound, ErrorCodes.PaymentNotFound))
            : Result<PaymentResponse>.Success(payment);
    }

    public Task<PagedResult<PaymentSummaryResponse>> SearchAsync(
        ManagedPaymentListCriteria criteria,
        CancellationToken cancellationToken) =>
        paymentReader.SearchAsync(criteria, cancellationToken);
}
