namespace ECommerce.Shipping.Application.Shipments;

public sealed record CreateShipmentResult(
    bool Succeeded,
    Guid? ShipmentId,
    string? TrackingNumber,
    string? ReasonCode,
    string? Reason);
