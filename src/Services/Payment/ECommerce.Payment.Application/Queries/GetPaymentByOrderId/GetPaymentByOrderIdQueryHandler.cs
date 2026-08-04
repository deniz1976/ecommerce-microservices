using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Payment.Application.Payments;

namespace ECommerce.Payment.Application.Queries.GetPaymentByOrderId;

public sealed class GetPaymentByOrderIdQueryHandler
    : IQueryHandler<GetPaymentByOrderIdQuery, Result<PaymentResponse>>
{
    private readonly PaymentQueryService queryService;

    public GetPaymentByOrderIdQueryHandler(PaymentQueryService queryService)
    {
        this.queryService = queryService;
    }

    public Task<Result<PaymentResponse>> HandleAsync(
        GetPaymentByOrderIdQuery query,
        CancellationToken cancellationToken)
    {
        return queryService.GetByOrderIdAsync(query.OrderId, cancellationToken);
    }
}
