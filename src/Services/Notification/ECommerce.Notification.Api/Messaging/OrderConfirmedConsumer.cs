using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Notification.Application.Commands.CreateNotification;
using ECommerce.Notification.Application.Notifications;
using MassTransit;
using MediatR;

namespace ECommerce.Notification.Api.Messaging;

public sealed class OrderConfirmedConsumer : IConsumer<OrderConfirmed>
{
    private readonly ISender sender;

    public OrderConfirmedConsumer(ISender sender)
    {
        this.sender = sender;
    }

    public Task Consume(ConsumeContext<OrderConfirmed> context)
    {
        (string title, string message) = NotificationText.OrderStatus(
            "order.confirmed",
            context.Message.OrderId,
            "Your order is confirmed.");
        return sender.Send(
            new CreateNotificationCommand(
                new CreateNotificationRequest(
                    context.Message.MessageId,
                    context.Message.CustomerId,
                    context.Message.OrderId,
                    "order.confirmed",
                    title,
                    message,
                    "en")),
            context.CancellationToken);
    }
}
