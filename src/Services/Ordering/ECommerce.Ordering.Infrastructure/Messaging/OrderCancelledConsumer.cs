using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Ordering.Application.Commands.ChangeOrderStatus;
using MassTransit;
using MediatR;

namespace ECommerce.Ordering.Infrastructure.Messaging;

public sealed class OrderCancelledConsumer : IConsumer<OrderCancelled>
{
    private readonly ISender sender;

    public OrderCancelledConsumer(ISender sender)
    {
        this.sender = sender;
    }

    public Task Consume(ConsumeContext<OrderCancelled> context)
    {
        return sender.Send(
            new ChangeOrderStatusCommand(
                context.Message.OrderId,
                context.Message.CustomerId,
                OrderStatusChange.Cancel,
                context.Message.ReasonCode),
            context.CancellationToken);
    }
}
