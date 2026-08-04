using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Notification.Application.Commands.CreateNotification;
using ECommerce.Notification.Application.Notifications;
using MassTransit;
using MediatR;

namespace ECommerce.Notification.Api.Messaging;

public sealed class PaymentAuthorizedConsumer : IConsumer<PaymentAuthorized>
{
    private readonly ISender sender;

    public PaymentAuthorizedConsumer(
        ISender sender)
    {
        this.sender = sender;
    }

    public Task Consume(ConsumeContext<PaymentAuthorized> context)
    {
        (string title, string message) = NotificationText.OrderStatus("payment.authorized", context.Message.OrderId, "Payment has been authorized.");
        return sender.Send(
            new CreateNotificationCommand(
                new CreateNotificationRequest(
                    context.Message.MessageId,
                    context.Message.CustomerId,
                    context.Message.OrderId,
                    "payment.authorized",
                    title,
                    message,
                    "en")),
            context.CancellationToken);
    }
}
