using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Orders;

namespace ECommerce.Ordering.Application.Queries.GetOrdersByCustomer;

public sealed class GetOrdersByCustomerQueryHandler
    : IQueryHandler<GetOrdersByCustomerQuery, Result<PagedResult<OrderSummaryResponse>>>
{
    private readonly OrderListQueryService queryService;

    public GetOrdersByCustomerQueryHandler(OrderListQueryService queryService)
    {
        this.queryService = queryService;
    }

    public Task<Result<PagedResult<OrderSummaryResponse>>> HandleAsync(
        GetOrdersByCustomerQuery query,
        CancellationToken cancellationToken)
    {
        return queryService.SearchAsync(
            new OrderListCriteria(
                query.CustomerId,
                query.PageNumber,
                query.PageSize,
                query.Status,
                query.SortDescending),
            cancellationToken);
    }
}
