using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Ordering.Application.Commands.ChangeOrderStatus;
using MassTransit;
using MediatR;

namespace ECommerce.Ordering.Infrastructure.Messaging;

public sealed class OrderCancellationRejectedConsumer(ISender sender)
    : IConsumer<OrderCancellationRejected>
{
    public Task Consume(ConsumeContext<OrderCancellationRejected> context)
    {
        return sender.Send(
            new ChangeOrderStatusCommand(
                context.Message.OrderId,
                context.Message.CustomerId,
                OrderStatusChange.RejectCancellation),
            context.CancellationToken);
    }
}
