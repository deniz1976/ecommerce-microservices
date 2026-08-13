using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.Application.Orders;

public sealed class OrderDetailQueryService
{
    private readonly IRepository<Order, Guid> repository;

    public OrderDetailQueryService(IRepository<Order, Guid> repository)
    {
        this.repository = repository;
    }

    public async Task<Result<OrderResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        Order? order = await repository.GetByIdAsync(id, cancellationToken);

        return order is null
            ? Result<OrderResponse>.Failure(
                new Error(ErrorCodes.OrderNotFound, ErrorCodes.OrderNotFound))
            : Result<OrderResponse>.Success(order.ToResponse());
    }
}
