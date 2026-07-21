using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Notification.Application.Notifications;
using MassTransit;

namespace ECommerce.Notification.Api.Messaging;

public sealed class PaymentFailedConsumer : IConsumer<PaymentFailed>
{
    private readonly NotificationService notificationService;

    public PaymentFailedConsumer(NotificationService notificationService)
    {
        this.notificationService = notificationService;
    }

    public Task Consume(ConsumeContext<PaymentFailed> context)
    {
        (string title, string message) = NotificationText.OrderStatus("payment.failed", context.Message.OrderId, $"Payment failed: {context.Message.Reason}");
        return notificationService.CreateAsync(new CreateNotificationRequest(context.Message.MessageId, context.Message.CustomerId, context.Message.OrderId, "payment.failed", title, message, "en"), context.CancellationToken);
    }
}
