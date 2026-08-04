using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Ordering.Application.Commands.ChangeOrderStatus;
using MassTransit;
using MediatR;

namespace ECommerce.Ordering.Infrastructure.Messaging;

public sealed class PaymentAuthorizedConsumer : IConsumer<PaymentAuthorized>
{
    private readonly ISender sender;

    public PaymentAuthorizedConsumer(ISender sender)
    {
        this.sender = sender;
    }

    public Task Consume(ConsumeContext<PaymentAuthorized> context)
    {
        return sender.Send(
            new ChangeOrderStatusCommand(
                context.Message.OrderId,
                context.Message.CustomerId,
                OrderStatusChange.PaymentAuthorized),
            context.CancellationToken);
    }
}
