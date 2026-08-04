using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Notification.Application.Commands.CreateNotification;
using ECommerce.Notification.Application.Notifications;
using MassTransit;
using MediatR;

namespace ECommerce.Notification.Api.Messaging;

public sealed class ShipmentFailedConsumer : IConsumer<ShipmentFailed>
{
    private readonly ISender sender;

    public ShipmentFailedConsumer(
        ISender sender)
    {
        this.sender = sender;
    }

    public Task Consume(ConsumeContext<ShipmentFailed> context)
    {
        (string title, string message) = NotificationText.OrderStatus("shipment.failed", context.Message.OrderId, $"Shipment failed: {context.Message.Reason}");
        return sender.Send(
            new CreateNotificationCommand(
                new CreateNotificationRequest(
                    context.Message.MessageId,
                    context.Message.CustomerId,
                    context.Message.OrderId,
                    "shipment.failed",
                    title,
                    message,
                    "en")),
            context.CancellationToken);
    }
}
