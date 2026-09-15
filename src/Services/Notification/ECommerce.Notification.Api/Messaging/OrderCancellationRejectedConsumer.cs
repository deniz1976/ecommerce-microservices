using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Notification.Application.Commands.CreateNotification;
using ECommerce.Notification.Application.Notifications;
using MassTransit;
using MediatR;

namespace ECommerce.Notification.Api.Messaging;

public sealed class OrderCancellationRejectedConsumer : IConsumer<OrderCancellationRejected>
{
    private readonly ISender sender;

    public OrderCancellationRejectedConsumer(ISender sender)
    {
        this.sender = sender;
    }

    public Task Consume(ConsumeContext<OrderCancellationRejected> context)
    {
        (string title, string message) = NotificationText.OrderStatus(
            "order.cancellation_rejected",
            context.Message.OrderId,
            "Your cancellation request was rejected because the order already progressed.");
        return sender.Send(
            new CreateNotificationCommand(
                new CreateNotificationRequest(
                    context.Message.MessageId,
                    context.Message.CustomerId,
                    context.Message.OrderId,
                    "order.cancellation_rejected",
                    title,
                    message,
                    "en")),
            context.CancellationToken);
    }
}
