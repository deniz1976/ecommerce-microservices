using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Ordering.Application.Commands.ChangeOrderStatus;
using MassTransit;

namespace ECommerce.Ordering.Infrastructure.Messaging;

public sealed class ShipmentCreatedConsumer : IConsumer<ShipmentCreated>
{
    private readonly ICommandHandler<ChangeOrderStatusCommand> commandHandler;

    public ShipmentCreatedConsumer(ICommandHandler<ChangeOrderStatusCommand> commandHandler)
    {
        this.commandHandler = commandHandler;
    }

    public Task Consume(ConsumeContext<ShipmentCreated> context)
    {
        return commandHandler.HandleAsync(
            new ChangeOrderStatusCommand(
                context.Message.OrderId,
                context.Message.CustomerId,
                OrderStatusChange.ShipmentCreated),
            context.CancellationToken);
    }
}
