using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Notification.Application.Commands.CreateNotification;
using ECommerce.Notification.Application.Notifications;
using MassTransit;
using MediatR;

namespace ECommerce.Notification.Api.Messaging;

public sealed class OrderCancellationRequestedConsumer : IConsumer<OrderCancellationRequested>
{
    private readonly ISender sender;

    public OrderCancellationRequestedConsumer(ISender sender)
    {
        this.sender = sender;
    }

    public Task Consume(ConsumeContext<OrderCancellationRequested> context)
    {
        (string title, string message) = NotificationText.OrderStatus(
            "order.cancellation_requested",
            context.Message.OrderId,
            "Your cancellation request was received.");
        return sender.Send(
            new CreateNotificationCommand(
                new CreateNotificationRequest(
                    context.Message.MessageId,
                    context.Message.CustomerId,
                    context.Message.OrderId,
                    "order.cancellation_requested",
                    title,
                    message,
                    "en")),
            context.CancellationToken);
    }
}
