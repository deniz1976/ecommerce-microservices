using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Notification.Application.Commands.CreateNotification;
using ECommerce.Notification.Application.Notifications;
using MassTransit;

namespace ECommerce.Notification.Api.Messaging;

public sealed class PaymentFailedConsumer : IConsumer<PaymentFailed>
{
    private readonly ICommandHandler<CreateNotificationCommand, NotificationMessage> commandHandler;

    public PaymentFailedConsumer(
        ICommandHandler<CreateNotificationCommand, NotificationMessage> commandHandler)
    {
        this.commandHandler = commandHandler;
    }

    public Task Consume(ConsumeContext<PaymentFailed> context)
    {
        (string title, string message) = NotificationText.OrderStatus("payment.failed", context.Message.OrderId, $"Payment failed: {context.Message.Reason}");
        return commandHandler.HandleAsync(
            new CreateNotificationCommand(
                new CreateNotificationRequest(
                    context.Message.MessageId,
                    context.Message.CustomerId,
                    context.Message.OrderId,
                    "payment.failed",
                    title,
                    message,
                    "en")),
            context.CancellationToken);
    }
}
