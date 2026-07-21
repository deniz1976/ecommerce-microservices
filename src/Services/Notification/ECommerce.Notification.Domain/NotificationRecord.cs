namespace ECommerce.Notification.Domain;

public sealed class NotificationRecord
{
    private NotificationRecord()
    {
        Type = string.Empty;
        Title = string.Empty;
        Message = string.Empty;
        Culture = string.Empty;
    }

    public NotificationRecord(
        Guid sourceMessageId,
        Guid customerId,
        Guid? orderId,
        string type,
        string title,
        string message,
        string culture,
        NotificationChannel channel)
    {
        Id = Guid.NewGuid();
        SourceMessageId = sourceMessageId;
        CustomerId = customerId;
        OrderId = orderId;
        Type = type;
        Title = title;
        Message = message;
        Culture = culture;
        Channel = channel;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid SourceMessageId { get; private set; }

    public Guid CustomerId { get; private set; }

    public Guid? OrderId { get; private set; }

    public string Type { get; private set; }

    public string Title { get; private set; }

    public string Message { get; private set; }

    public string Culture { get; private set; }

    public NotificationChannel Channel { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
}
