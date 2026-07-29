using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Orders;

namespace ECommerce.Ordering.Application.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler
    : IQueryHandler<GetOrderByIdQuery, Result<OrderResponse>>
{
    private readonly OrderService orderService;

    public GetOrderByIdQueryHandler(OrderService orderService)
    {
        this.orderService = orderService;
    }

    public Task<Result<OrderResponse>> HandleAsync(
        GetOrderByIdQuery query,
        CancellationToken cancellationToken)
    {
        return orderService.GetByIdAsync(query.Id, cancellationToken);
    }
}
