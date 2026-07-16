using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.Notification.Application.Notifications;
using MassTransit;

namespace ECommerce.Notification.Api.Messaging;

public sealed class OrderSubmittedConsumer : IConsumer<OrderSubmitted>
{
    private readonly NotificationService notificationService;

    public OrderSubmittedConsumer(NotificationService notificationService)
    {
        this.notificationService = notificationService;
    }

    public Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        (string title, string message) = NotificationText.OrderStatus("order.submitted", context.Message.OrderId, "Your order has been submitted.");
        return notificationService.CreateAsync(new CreateNotificationRequest(context.Message.CustomerId, context.Message.OrderId, "order.submitted", title, message, "en"), context.CancellationToken);
    }
}
