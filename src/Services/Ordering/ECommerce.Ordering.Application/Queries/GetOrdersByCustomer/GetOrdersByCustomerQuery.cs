using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Orders;
using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.Application.Queries.GetOrdersByCustomer;

public sealed record GetOrdersByCustomerQuery(
    Guid CustomerId,
    int PageNumber,
    int PageSize,
    OrderStatus? Status,
    bool SortDescending)
    : IQuery<Result<PagedResult<OrderSummaryResponse>>>;
