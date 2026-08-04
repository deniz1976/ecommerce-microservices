using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.Application.Orders;

public sealed class OrderCancellationService(
    IRepository<Order, Guid> repository,
    IUnitOfWork unitOfWork,
    IOrderCancellationRequestedPublisher publisher)
{
    public async Task<Result<OrderResponse>> RequestAsync(
        Guid orderId,
        Guid correlationId,
        Guid? causationId,
        CancellationToken cancellationToken)
    {
        Order? order = await repository.GetByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            return Result<OrderResponse>.Failure(
                new Error(ErrorCodes.OrderNotFound, ErrorCodes.OrderNotFound));
        }

        if (order.Status is OrderStatus.Cancelled or OrderStatus.CancellationRequested)
        {
            return Result<OrderResponse>.Success(order.ToResponse());
        }

        if (order.Status is OrderStatus.PaymentAuthorized or
            OrderStatus.ShipmentCreated or
            OrderStatus.Confirmed)
        {
            return Result<OrderResponse>.Failure(
                new Error(ErrorCodes.OrderNotCancellable, ErrorCodes.OrderNotCancellable));
        }

        order.MarkCancellationRequested();
        await publisher.PublishAsync(
            order,
            correlationId,
            causationId,
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<OrderResponse>.Success(order.ToResponse());
    }
}
