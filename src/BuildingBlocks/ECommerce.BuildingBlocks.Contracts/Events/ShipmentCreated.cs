using ECommerce.BuildingBlocks.Contracts.Messaging;

namespace ECommerce.BuildingBlocks.Contracts.Events;

public sealed record ShipmentCreated(
    Guid MessageId,
    Guid CorrelationId,
    Guid? CausationId,
    DateTimeOffset OccurredAt,
    int Version,
    Guid OrderId,
    Guid CustomerId,
    Guid ShipmentId,
    string TrackingNumber) : IEvent;
