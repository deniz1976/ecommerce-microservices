using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Ordering.Application.Commands.ChangeOrderStatus;
using MassTransit;

namespace ECommerce.Ordering.Infrastructure.Messaging;

public sealed class InventoryReservedConsumer : IConsumer<InventoryReserved>
{
    private readonly ICommandHandler<ChangeOrderStatusCommand> commandHandler;

    public InventoryReservedConsumer(ICommandHandler<ChangeOrderStatusCommand> commandHandler)
    {
        this.commandHandler = commandHandler;
    }

    public Task Consume(ConsumeContext<InventoryReserved> context)
    {
        return commandHandler.HandleAsync(
            new ChangeOrderStatusCommand(
                context.Message.OrderId,
                context.Message.CustomerId,
                OrderStatusChange.InventoryReserved),
            context.CancellationToken);
    }
}
