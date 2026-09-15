using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Notification.Application.Commands.CreateNotification;
using ECommerce.Notification.Application.Notifications;
using MassTransit;
using MediatR;

namespace ECommerce.Notification.Api.Messaging;

public sealed class PaymentFailedConsumer : IConsumer<PaymentFailed>
{
    private readonly ISender sender;

    public PaymentFailedConsumer(
        ISender sender)
    {
        this.sender = sender;
    }

    public Task Consume(ConsumeContext<PaymentFailed> context)
    {
        (string title, string message) = NotificationText.OrderStatus("payment.failed", context.Message.OrderId, $"Payment failed: {context.Message.Reason}");
        return sender.Send(
            new CreateNotificationCommand(
                new CreateNotificationRequest(
                    context.Message.MessageId,
                    context.Message.CustomerId,
                    context.Message.OrderId,
                    "payment.failed",
                    title,
                    message,
                    "en",
                    context.Message.ReasonCode,
                    null)),
            context.CancellationToken);
    }
}
