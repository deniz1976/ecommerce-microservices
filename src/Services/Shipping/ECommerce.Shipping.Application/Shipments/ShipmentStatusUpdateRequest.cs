using ECommerce.Shipping.Domain;

namespace ECommerce.Shipping.Application.Shipments;

public sealed record ShipmentStatusUpdateRequest(
    Guid UpdateId,
    string TrackingNumber,
    ShipmentStatus Status,
    DateTimeOffset OccurredAt);
