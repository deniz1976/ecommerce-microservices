using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Notification.Application.Commands.CreateNotification;
using ECommerce.Notification.Application.Notifications;
using MassTransit;

namespace ECommerce.Notification.Api.Messaging;

public sealed class ShipmentFailedConsumer : IConsumer<ShipmentFailed>
{
    private readonly ICommandHandler<CreateNotificationCommand, NotificationMessage> commandHandler;

    public ShipmentFailedConsumer(
        ICommandHandler<CreateNotificationCommand, NotificationMessage> commandHandler)
    {
        this.commandHandler = commandHandler;
    }

    public Task Consume(ConsumeContext<ShipmentFailed> context)
    {
        (string title, string message) = NotificationText.OrderStatus("shipment.failed", context.Message.OrderId, $"Shipment failed: {context.Message.Reason}");
        return commandHandler.HandleAsync(
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
