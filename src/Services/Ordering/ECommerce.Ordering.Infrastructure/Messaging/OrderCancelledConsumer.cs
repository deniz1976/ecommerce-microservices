using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Ordering.Application.Commands.ChangeOrderStatus;
using MassTransit;

namespace ECommerce.Ordering.Infrastructure.Messaging;

public sealed class OrderCancelledConsumer : IConsumer<OrderCancelled>
{
    private readonly ICommandHandler<ChangeOrderStatusCommand> commandHandler;

    public OrderCancelledConsumer(ICommandHandler<ChangeOrderStatusCommand> commandHandler)
    {
        this.commandHandler = commandHandler;
    }

    public Task Consume(ConsumeContext<OrderCancelled> context)
    {
        return commandHandler.HandleAsync(
            new ChangeOrderStatusCommand(
                context.Message.OrderId,
                context.Message.CustomerId,
                OrderStatusChange.Cancel,
                context.Message.ReasonCode),
            context.CancellationToken);
    }
}
