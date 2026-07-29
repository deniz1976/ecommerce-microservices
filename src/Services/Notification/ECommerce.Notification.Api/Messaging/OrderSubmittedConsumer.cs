using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Notification.Application.Commands.CreateNotification;
using ECommerce.Notification.Application.Notifications;
using MassTransit;

namespace ECommerce.Notification.Api.Messaging;

public sealed class OrderSubmittedConsumer : IConsumer<OrderSubmitted>
{
    private readonly ICommandHandler<CreateNotificationCommand, NotificationMessage> commandHandler;

    public OrderSubmittedConsumer(
        ICommandHandler<CreateNotificationCommand, NotificationMessage> commandHandler)
    {
        this.commandHandler = commandHandler;
    }

    public Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        (string title, string message) = NotificationText.OrderStatus("order.submitted", context.Message.OrderId, "Your order has been submitted.");
        return commandHandler.HandleAsync(
            new CreateNotificationCommand(
                new CreateNotificationRequest(
                    context.Message.MessageId,
                    context.Message.CustomerId,
                    context.Message.OrderId,
                    "order.submitted",
                    title,
                    message,
                    "en")),
            context.CancellationToken);
    }
}
