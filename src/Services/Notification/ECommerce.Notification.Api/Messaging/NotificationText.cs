namespace ECommerce.Notification.Api.Messaging;

public static class NotificationText
{
    public static (string Title, string Message) OrderStatus(string type, Guid orderId, string detail)
    {
        string shortOrderId = orderId.ToString("N")[..8].ToUpperInvariant();
        return ($"Order {shortOrderId}", detail);
    }
}
