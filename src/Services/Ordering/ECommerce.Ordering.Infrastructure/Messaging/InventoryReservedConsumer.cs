using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Ordering.Application.Commands.ChangeOrderStatus;
using MassTransit;
using MediatR;

namespace ECommerce.Ordering.Infrastructure.Messaging;

public sealed class InventoryReservedConsumer : IConsumer<InventoryReserved>
{
    private readonly ISender sender;

    public InventoryReservedConsumer(ISender sender)
    {
        this.sender = sender;
    }

    public Task Consume(ConsumeContext<InventoryReserved> context)
    {
        return sender.Send(
            new ChangeOrderStatusCommand(
                context.Message.OrderId,
                context.Message.CustomerId,
                OrderStatusChange.InventoryReserved),
            context.CancellationToken);
    }
}
