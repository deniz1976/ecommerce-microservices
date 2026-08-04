using ECommerce.BuildingBlocks.Contracts.Commands;
using ECommerce.Payment.Application.Commands.RefundPayment;
using ECommerce.Payment.Application.Payments;
using MassTransit;
using MediatR;

namespace ECommerce.Payment.Infrastructure.Messaging;

public sealed class RefundPaymentConsumer : IConsumer<RefundPayment>
{
    private readonly ISender sender;

    public RefundPaymentConsumer(ISender sender)
    {
        this.sender = sender;
    }

    public Task Consume(ConsumeContext<RefundPayment> context)
    {
        return sender.Send(
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
