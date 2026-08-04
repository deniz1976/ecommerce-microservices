using ECommerce.Shipping.Domain;

namespace ECommerce.Shipping.Application.Shipments;

public sealed record ManagedShipmentListCriteria(
    int PageNumber,
    int PageSize,
    Guid? CustomerId,
    Guid? OrderId,
    ShipmentStatus? Status,
    DateTimeOffset? CreatedFrom,
    DateTimeOffset? CreatedTo,
    string? SortBy,
    bool SortDescending);
