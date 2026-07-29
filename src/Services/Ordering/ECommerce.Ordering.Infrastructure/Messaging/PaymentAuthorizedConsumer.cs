using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Ordering.Application.Commands.ChangeOrderStatus;
using MassTransit;

namespace ECommerce.Ordering.Infrastructure.Messaging;

public sealed class PaymentAuthorizedConsumer : IConsumer<PaymentAuthorized>
{
    private readonly ICommandHandler<ChangeOrderStatusCommand> commandHandler;

    public PaymentAuthorizedConsumer(ICommandHandler<ChangeOrderStatusCommand> commandHandler)
    {
        this.commandHandler = commandHandler;
    }

    public Task Consume(ConsumeContext<PaymentAuthorized> context)
    {
        return commandHandler.HandleAsync(
            new ChangeOrderStatusCommand(
                context.Message.OrderId,
                context.Message.CustomerId,
                OrderStatusChange.PaymentAuthorized),
            context.CancellationToken);
    }
}
