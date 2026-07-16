using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Notification.Application.Notifications;
using MassTransit;

namespace ECommerce.Notification.Api.Messaging;

public sealed class ShipmentCreatedConsumer : IConsumer<ShipmentCreated>
{
    private readonly NotificationService notificationService;

    public ShipmentCreatedConsumer(NotificationService notificationService)
    {
        this.notificationService = notificationService;
    }

    public Task Consume(ConsumeContext<ShipmentCreated> context)
    {
        (string title, string message) = NotificationText.OrderStatus("shipment.created", context.Message.OrderId, $"Shipment created. Tracking number: {context.Message.TrackingNumber}");
        return notificationService.CreateAsync(new CreateNotificationRequest(context.Message.CustomerId, context.Message.OrderId, "shipment.created", title, message, "en"), context.CancellationToken);
    }
}
