using ECommerce.Shipping.Application.Shipments;
using ECommerce.Shipping.Domain;

namespace ECommerce.Shipping.UnitTests;

public sealed class ShipmentStatusUpdateTests
{
    [Fact]
    public void ApplyStatusUpdateAdvancesMonotonicallyAndNormalizesUtc()
    {
        Shipment shipment = CreateShipment();
        DateTimeOffset occurredAt = shipment.StatusUpdatedAt!.Value
            .AddMinutes(1)
            .ToOffset(TimeSpan.FromHours(3));

        ShipmentStatusUpdateResult result = shipment.ApplyStatusUpdate(
            Guid.NewGuid(),
            ShipmentStatus.InTransit,
            occurredAt);

        Assert.Equal(ShipmentStatusUpdateResult.Applied, result);
        Assert.Equal(ShipmentStatus.InTransit, shipment.Status);
        Assert.Equal(TimeSpan.Zero, shipment.StatusUpdatedAt!.Value.Offset);
        Assert.Equal(1, shipment.Version);
    }

    [Fact]
    public void DuplicateAndOlderUpdatesDoNotMutateShipment()
    {
        Shipment shipment = CreateShipment();
        Guid updateId = Guid.NewGuid();
        DateTimeOffset firstAt = shipment.StatusUpdatedAt!.Value.AddMinutes(2);
        shipment.ApplyStatusUpdate(updateId, ShipmentStatus.InTransit, firstAt);

        ShipmentStatusUpdateResult duplicate = shipment.ApplyStatusUpdate(
            updateId,
            ShipmentStatus.Delivered,
            firstAt.AddMinutes(1));
        ShipmentStatusUpdateResult stale = shipment.ApplyStatusUpdate(
            Guid.NewGuid(),
            ShipmentStatus.Delivered,
            firstAt.AddMinutes(-1));

        Assert.Equal(ShipmentStatusUpdateResult.Duplicate, duplicate);
        Assert.Equal(ShipmentStatusUpdateResult.Stale, stale);
        Assert.Equal(ShipmentStatus.InTransit, shipment.Status);
        Assert.Equal(1, shipment.Version);
    }

    [Fact]
    public void TerminalShipmentIgnoresLateUpdates()
    {
        Shipment shipment = CreateShipment();
        DateTimeOffset deliveredAt = shipment.StatusUpdatedAt!.Value.AddMinutes(2);
        shipment.ApplyStatusUpdate(Guid.NewGuid(), ShipmentStatus.Delivered, deliveredAt);

        ShipmentStatusUpdateResult result = shipment.ApplyStatusUpdate(
            Guid.NewGuid(),
            ShipmentStatus.Failed,
            deliveredAt.AddMinutes(1));

        Assert.Equal(ShipmentStatusUpdateResult.Terminal, result);
        Assert.Equal(ShipmentStatus.Delivered, shipment.Status);
        Assert.Equal(1, shipment.Version);
    }

    [Fact]
    public async Task UpdateServiceCommitsOnlyAppliedTransition()
    {
        FakeShipmentRepository repository = new();
        Shipment shipment = CreateShipment();
        repository.Add(shipment);
        ShipmentStatusUpdateService service = new(repository, repository, repository);
        ShipmentStatusUpdateRequest request = new(
            Guid.NewGuid(),
            shipment.TrackingNumber!,
            ShipmentStatus.InTransit,
            shipment.StatusUpdatedAt!.Value.AddMinutes(1));

        ShipmentStatusUpdateResult applied = await service.UpdateAsync(
            request,
            CancellationToken.None);
        ShipmentStatusUpdateResult duplicate = await service.UpdateAsync(
            request,
            CancellationToken.None);

        Assert.Equal(ShipmentStatusUpdateResult.Applied, applied);
        Assert.Equal(ShipmentStatusUpdateResult.Duplicate, duplicate);
        Assert.Equal(1, repository.SaveChangesCount);
    }

    private static Shipment CreateShipment()
    {
        ShipmentAddress.TryCreate(
            "Test Customer",
            "Runtime Avenue 1",
            "Istanbul",
            "TR",
            "34000",
            out ShipmentAddress? address);
        return Shipment.Create(Guid.NewGuid(), Guid.NewGuid(), address!, "TRACK-123");
    }
}
