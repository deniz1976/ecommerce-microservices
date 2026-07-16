using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Notification.Application.Notifications;
using MassTransit;

namespace ECommerce.Notification.Api.Messaging;

public sealed class PaymentAuthorizedConsumer : IConsumer<PaymentAuthorized>
{
    private readonly NotificationService notificationService;

    public PaymentAuthorizedConsumer(NotificationService notificationService)
    {
        this.notificationService = notificationService;
    }

    public Task Consume(ConsumeContext<PaymentAuthorized> context)
    {
        (string title, string message) = NotificationText.OrderStatus("payment.authorized", context.Message.OrderId, "Payment has been authorized.");
        return notificationService.CreateAsync(new CreateNotificationRequest(context.Message.CustomerId, context.Message.OrderId, "payment.authorized", title, message, "en"), context.CancellationToken);
    }
}
