using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Notification.Application.Commands.CreateNotification;
using ECommerce.Notification.Application.Notifications;
using MassTransit;

namespace ECommerce.Notification.Api.Messaging;

public sealed class ShipmentCreatedConsumer : IConsumer<ShipmentCreated>
{
    private readonly ICommandHandler<CreateNotificationCommand, NotificationMessage> commandHandler;

    public ShipmentCreatedConsumer(
        ICommandHandler<CreateNotificationCommand, NotificationMessage> commandHandler)
    {
        this.commandHandler = commandHandler;
    }

    public Task Consume(ConsumeContext<ShipmentCreated> context)
    {
        (string title, string message) = NotificationText.OrderStatus("shipment.created", context.Message.OrderId, $"Shipment created. Tracking number: {context.Message.TrackingNumber}");
        return commandHandler.HandleAsync(
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
