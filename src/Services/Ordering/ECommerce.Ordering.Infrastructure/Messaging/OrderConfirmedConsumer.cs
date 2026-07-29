using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Ordering.Application.Commands.ChangeOrderStatus;
using MassTransit;

namespace ECommerce.Ordering.Infrastructure.Messaging;

public sealed class OrderConfirmedConsumer : IConsumer<OrderConfirmed>
{
    private readonly ICommandHandler<ChangeOrderStatusCommand> commandHandler;

    public OrderConfirmedConsumer(ICommandHandler<ChangeOrderStatusCommand> commandHandler)
    {
        this.commandHandler = commandHandler;
    }

    public Task Consume(ConsumeContext<OrderConfirmed> context)
    {
        return commandHandler.HandleAsync(
            new ChangeOrderStatusCommand(
                context.Message.OrderId,
                context.Message.CustomerId,
                OrderStatusChange.Confirm),
            context.CancellationToken);
    }
}
