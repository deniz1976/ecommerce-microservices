using ECommerce.Shipping.Domain;

namespace ECommerce.Shipping.Application.Shipments;

public sealed record ShipmentResponse(
    Guid Id,
    Guid OrderId,
    Guid CustomerId,
    string? TrackingNumber,
    ShipmentStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
