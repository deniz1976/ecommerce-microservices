using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Orders;
using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.Application.Queries.SearchSellerOrders;

public sealed record SearchSellerOrdersQuery(
    Guid StoreId,
    int PageNumber,
    int PageSize,
    OrderStatus? Status,
    bool SortDescending,
    SellerOrderAccessContext AccessContext)
    : IQuery<Result<PagedResult<SellerOrderSummaryResponse>>>;
