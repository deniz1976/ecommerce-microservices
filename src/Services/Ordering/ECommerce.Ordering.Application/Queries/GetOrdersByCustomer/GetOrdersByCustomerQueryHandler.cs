using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Orders;

namespace ECommerce.Ordering.Application.Queries.GetOrdersByCustomer;

public sealed class GetOrdersByCustomerQueryHandler
    : IQueryHandler<GetOrdersByCustomerQuery, Result<IReadOnlyCollection<OrderResponse>>>
{
    private readonly OrderService orderService;

    public GetOrdersByCustomerQueryHandler(OrderService orderService)
    {
        this.orderService = orderService;
    }

    public Task<Result<IReadOnlyCollection<OrderResponse>>> HandleAsync(
        GetOrdersByCustomerQuery query,
        CancellationToken cancellationToken)
    {
        return orderService.GetByCustomerIdAsync(query.CustomerId, cancellationToken);
    }
}
