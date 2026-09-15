using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Notification.Application.Commands.CreateNotification;
using ECommerce.Notification.Application.Notifications;
using MassTransit;
using MediatR;

namespace ECommerce.Notification.Api.Messaging;

public sealed class OrderCancelledConsumer : IConsumer<OrderCancelled>
{
    private readonly ISender sender;

    public OrderCancelledConsumer(ISender sender)
    {
        this.sender = sender;
    }

    public Task Consume(ConsumeContext<OrderCancelled> context)
    {
        (string title, string message) = NotificationText.OrderStatus(
            "order.cancelled",
            context.Message.OrderId,
            $"Order cancelled: {context.Message.Reason}");
        return sender.Send(
            new CreateNotificationCommand(
                new CreateNotificationRequest(
                    context.Message.MessageId,
                    context.Message.CustomerId,
                    context.Message.OrderId,
                    "order.cancelled",
                    title,
                    message,
                    "en",
                    context.Message.ReasonCode,
                    null)),
            context.CancellationToken);
    }
}
