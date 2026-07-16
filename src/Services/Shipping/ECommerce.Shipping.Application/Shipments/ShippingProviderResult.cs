namespace ECommerce.Shipping.Application.Shipments;

public sealed record ShippingProviderResult(
    bool Succeeded,
    string? TrackingNumber,
    string? FailureReason);
