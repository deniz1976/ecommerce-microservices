using ECommerce.BuildingBlocks.Contracts.Inventory;
using ECommerce.BuildingBlocks.Contracts.Messaging;

namespace ECommerce.BuildingBlocks.Contracts.Commands;

public sealed record ReserveInventory(
    Guid MessageId,
    Guid CorrelationId,
    Guid? CausationId,
    DateTimeOffset OccurredAt,
    int Version,
    Guid OrderId,
    Guid CustomerId,
    IReadOnlyCollection<InventoryReservationLine> Items) : ICommand;
