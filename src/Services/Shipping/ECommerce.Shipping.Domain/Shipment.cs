namespace ECommerce.Shipping.Domain;

public sealed class Shipment
{
    private Shipment()
    {
        RecipientName = string.Empty;
        AddressLine = string.Empty;
        City = string.Empty;
        CountryCode = string.Empty;
        PostalCode = string.Empty;
        TrackingNumber = string.Empty;
    }

    private Shipment(
        Guid orderId,
        Guid customerId,
        string recipientName,
        string addressLine,
        string city,
        string countryCode,
        string postalCode,
        string trackingNumber,
        ShipmentStatus status,
        string? failureReason)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        CustomerId = customerId;
        RecipientName = recipientName;
        AddressLine = addressLine;
        City = city;
        CountryCode = countryCode;
        PostalCode = postalCode;
        TrackingNumber = trackingNumber;
        Status = status;
        FailureReason = failureReason;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public Guid CustomerId { get; private set; }

    public string RecipientName { get; private set; }

    public string AddressLine { get; private set; }

    public string City { get; private set; }

    public string CountryCode { get; private set; }

    public string PostalCode { get; private set; }

    public string TrackingNumber { get; private set; }

    public ShipmentStatus Status { get; private set; }

    public string? FailureReason { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Shipment Create(
        Guid orderId,
        Guid customerId,
        string recipientName,
        string addressLine,
        string city,
        string countryCode,
        string postalCode,
        string trackingNumber)
    {
        return new Shipment(
            orderId,
            customerId,
            recipientName,
            addressLine,
            city,
            countryCode,
            postalCode,
            trackingNumber,
            ShipmentStatus.Created,
            null);
    }

    public static Shipment CreateFailed(
        Guid orderId,
        Guid customerId,
        string recipientName,
        string addressLine,
        string city,
        string countryCode,
        string postalCode,
        string reason)
    {
        return new Shipment(
            orderId,
            customerId,
            recipientName,
            addressLine,
            city,
            countryCode,
            postalCode,
            string.Empty,
            ShipmentStatus.Failed,
            reason);
    }
}
