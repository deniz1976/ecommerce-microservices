using ECommerce.BuildingBlocks.Contracts.Messaging;

namespace ECommerce.BuildingBlocks.Contracts.Commands;

public sealed record UpdateShipmentStatus(
    Guid MessageId,
    Guid CorrelationId,
    Guid? CausationId,
    DateTimeOffset OccurredAt,
    int Version,
    string TrackingNumber,
    ShipmentProgressStatus Status) : ICommand;
