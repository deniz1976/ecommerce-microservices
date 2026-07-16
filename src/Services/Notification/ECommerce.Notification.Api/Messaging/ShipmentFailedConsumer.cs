using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Notification.Application.Notifications;
using MassTransit;

namespace ECommerce.Notification.Api.Messaging;

public sealed class ShipmentFailedConsumer : IConsumer<ShipmentFailed>
{
    private readonly NotificationService notificationService;

    public ShipmentFailedConsumer(NotificationService notificationService)
    {
        this.notificationService = notificationService;
    }

    public Task Consume(ConsumeContext<ShipmentFailed> context)
    {
        (string title, string message) = NotificationText.OrderStatus("shipment.failed", context.Message.OrderId, $"Shipment failed: {context.Message.Reason}");
        return notificationService.CreateAsync(new CreateNotificationRequest(context.Message.CustomerId, context.Message.OrderId, "shipment.failed", title, message, "en"), context.CancellationToken);
    }
}
