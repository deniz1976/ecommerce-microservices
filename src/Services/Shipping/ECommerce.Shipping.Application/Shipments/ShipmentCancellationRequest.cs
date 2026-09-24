namespace ECommerce.Shipping.Application.Shipments;

public sealed record ShipmentCancellationRequest(Guid OrderId, string Reason);
