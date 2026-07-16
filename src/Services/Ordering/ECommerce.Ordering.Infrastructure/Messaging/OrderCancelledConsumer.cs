using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Ordering.Application.Orders;
using MassTransit;

namespace ECommerce.Ordering.Infrastructure.Messaging;

public sealed class OrderCancelledConsumer : IConsumer<OrderCancelled>
{
    private readonly OrderStatusService orderStatusService;

    public OrderCancelledConsumer(OrderStatusService orderStatusService)
    {
        this.orderStatusService = orderStatusService;
    }

    public Task Consume(ConsumeContext<OrderCancelled> context)
    {
        return orderStatusService.CancelAsync(
            context.Message.OrderId,
            context.Message.CustomerId,
            context.CancellationToken);
    }
}
