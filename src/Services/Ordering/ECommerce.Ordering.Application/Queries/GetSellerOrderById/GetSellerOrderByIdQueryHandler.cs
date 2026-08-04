using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Orders;

namespace ECommerce.Ordering.Application.Queries.GetSellerOrderById;

public sealed class GetSellerOrderByIdQueryHandler
    : IQueryHandler<GetSellerOrderByIdQuery, Result<SellerOrderDetailResponse>>
{
    private readonly OrderQueryService queryService;

    public GetSellerOrderByIdQueryHandler(OrderQueryService queryService)
    {
        this.queryService = queryService;
    }

    public Task<Result<SellerOrderDetailResponse>> HandleAsync(
        GetSellerOrderByIdQuery query,
        CancellationToken cancellationToken)
    {
        return queryService.GetSellerAsync(
            query.StoreId,
            query.OrderId,
            query.AccessContext,
            cancellationToken);
    }
}
