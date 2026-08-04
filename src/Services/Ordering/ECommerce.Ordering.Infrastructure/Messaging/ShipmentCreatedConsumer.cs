using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Ordering.Application.Commands.ChangeOrderStatus;
using MassTransit;
using MediatR;

namespace ECommerce.Ordering.Infrastructure.Messaging;

public sealed class ShipmentCreatedConsumer : IConsumer<ShipmentCreated>
{
    private readonly ISender sender;

    public ShipmentCreatedConsumer(ISender sender)
    {
        this.sender = sender;
    }

    public Task Consume(ConsumeContext<ShipmentCreated> context)
    {
        return sender.Send(
            new ChangeOrderStatusCommand(
                context.Message.OrderId,
                context.Message.CustomerId,
                OrderStatusChange.ShipmentCreated),
            context.CancellationToken);
    }
}
