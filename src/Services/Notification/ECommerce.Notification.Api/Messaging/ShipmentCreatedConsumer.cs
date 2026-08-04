using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Notification.Application.Commands.CreateNotification;
using ECommerce.Notification.Application.Notifications;
using MassTransit;
using MediatR;

namespace ECommerce.Notification.Api.Messaging;

public sealed class ShipmentCreatedConsumer : IConsumer<ShipmentCreated>
{
    private readonly ISender sender;

    public ShipmentCreatedConsumer(
        ISender sender)
    {
        this.sender = sender;
    }

    public Task Consume(ConsumeContext<ShipmentCreated> context)
    {
        (string title, string message) = NotificationText.OrderStatus("shipment.created", context.Message.OrderId, $"Shipment created. Tracking number: {context.Message.TrackingNumber}");
        return sender.Send(
            new CreateNotificationCommand(
                new CreateNotificationRequest(
                    context.Message.MessageId,
                    context.Message.CustomerId,
                    context.Message.OrderId,
                    "shipment.created",
                    title,
                    message,
                    "en")),
            context.CancellationToken);
    }
}
