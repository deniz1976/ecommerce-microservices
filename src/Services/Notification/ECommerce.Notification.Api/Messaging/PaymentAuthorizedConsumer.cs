using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Notification.Application.Commands.CreateNotification;
using ECommerce.Notification.Application.Notifications;
using MassTransit;

namespace ECommerce.Notification.Api.Messaging;

public sealed class PaymentAuthorizedConsumer : IConsumer<PaymentAuthorized>
{
    private readonly ICommandHandler<CreateNotificationCommand, NotificationMessage> commandHandler;

    public PaymentAuthorizedConsumer(
        ICommandHandler<CreateNotificationCommand, NotificationMessage> commandHandler)
    {
        this.commandHandler = commandHandler;
    }

    public Task Consume(ConsumeContext<PaymentAuthorized> context)
    {
        (string title, string message) = NotificationText.OrderStatus("payment.authorized", context.Message.OrderId, "Payment has been authorized.");
        return commandHandler.HandleAsync(
            new CreateNotificationCommand(
                new CreateNotificationRequest(
                    context.Message.MessageId,
                    context.Message.CustomerId,
                    context.Message.OrderId,
                    "payment.authorized",
                    title,
                    message,
                    "en")),
            context.CancellationToken);
    }
}
