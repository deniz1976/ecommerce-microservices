using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Orders;

namespace ECommerce.Ordering.Application.Queries.SearchManagedOrders;

public sealed class SearchManagedOrdersQueryHandler
    : IQueryHandler<SearchManagedOrdersQuery, Result<PagedResult<OrderSummaryResponse>>>
{
    private readonly OrderListQueryService queryService;

    public SearchManagedOrdersQueryHandler(OrderListQueryService queryService)
    {
        this.queryService = queryService;
    }

    public Task<Result<PagedResult<OrderSummaryResponse>>> HandleAsync(
        SearchManagedOrdersQuery query,
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
