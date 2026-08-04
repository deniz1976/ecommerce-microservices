using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Orders;

namespace ECommerce.Ordering.Application.Queries.SearchSellerOrders;

public sealed class SearchSellerOrdersQueryHandler
    : IQueryHandler<SearchSellerOrdersQuery, Result<PagedResult<SellerOrderSummaryResponse>>>
{
    private readonly OrderQueryService queryService;

    public SearchSellerOrdersQueryHandler(OrderQueryService queryService)
    {
        this.queryService = queryService;
    }

    public Task<Result<PagedResult<SellerOrderSummaryResponse>>> HandleAsync(
        SearchSellerOrdersQuery query,
        CancellationToken cancellationToken)
    {
        return queryService.SearchSellerAsync(
            new SellerOrderListCriteria(
                query.StoreId,
                query.PageNumber,
                query.PageSize,
                query.Status,
                query.SortDescending),
            query.AccessContext,
            cancellationToken);
    }
}
