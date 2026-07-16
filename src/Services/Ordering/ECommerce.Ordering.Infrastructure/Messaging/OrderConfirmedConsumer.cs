using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Ordering.Application.Orders;
using MassTransit;

namespace ECommerce.Ordering.Infrastructure.Messaging;

public sealed class OrderConfirmedConsumer : IConsumer<OrderConfirmed>
{
    private readonly OrderStatusService orderStatusService;

    public OrderConfirmedConsumer(OrderStatusService orderStatusService)
    {
        this.orderStatusService = orderStatusService;
    }

    public Task Consume(ConsumeContext<OrderConfirmed> context)
    {
        return orderStatusService.ConfirmAsync(
            context.Message.OrderId,
            context.Message.CustomerId,
            context.CancellationToken);
    }
}
