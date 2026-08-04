using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Payment.Application.Payments;

public interface IPaymentQueryReader
{
    Task<PaymentResponse?> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken);

    Task<PagedResult<PaymentSummaryResponse>> SearchAsync(
        ManagedPaymentListCriteria criteria,
        CancellationToken cancellationToken);
}
