using ECommerce.BuildingBlocks.Contracts.Commands;
using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Payment.Application.Commands.RefundPayment;
using ECommerce.Payment.Application.Payments;
using MassTransit;

namespace ECommerce.Payment.Infrastructure.Messaging;

public sealed class RefundPaymentConsumer : IConsumer<RefundPayment>
{
    private readonly ICommandHandler<RefundPaymentCommand> commandHandler;

    public RefundPaymentConsumer(ICommandHandler<RefundPaymentCommand> commandHandler)
    {
        this.commandHandler = commandHandler;
    }

    public Task Consume(ConsumeContext<RefundPayment> context)
    {
        return commandHandler.HandleAsync(
            new RefundPaymentCommand(
                new RefundPaymentRequest(
                    context.Message.OrderId,
                    context.Message.CustomerId,
                    context.Message.Amount,
                    context.Message.Currency,
                    context.Message.Reason)),
            context.CancellationToken);
    }
}
