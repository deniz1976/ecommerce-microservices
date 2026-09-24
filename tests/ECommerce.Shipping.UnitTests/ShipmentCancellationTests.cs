using ECommerce.Shipping.Application.Shipments;
using ECommerce.Shipping.Domain;

namespace ECommerce.Shipping.UnitTests;

public sealed class ShipmentCancellationTests
{
    [Fact]
    public async Task Created_shipment_is_cancelled_once()
    {
        FakeShipmentRepository repository = new();
        Shipment shipment = CreateShipment();
        repository.Add(shipment);
        ShipmentCancellationService service = new(repository, repository, repository);
        ShipmentCancellationRequest request = new(shipment.OrderId, "Order was cancelled.");

        ShipmentCancellationResult first = await service.CancelAsync(request, CancellationToken.None);
        ShipmentCancellationResult second = await service.CancelAsync(request, CancellationToken.None);

        Assert.Equal(ShipmentCancellationResult.Cancelled, first);
        Assert.Equal(ShipmentCancellationResult.AlreadyCancelled, second);
        Assert.Equal(ShipmentStatus.Cancelled, shipment.Status);
        Assert.Equal("Order was cancelled.", shipment.FailureReason);
        Assert.Equal(1, repository.SaveChangesCount);
    }

    [Fact]
    public async Task Shipment_in_transit_is_not_cancelled()
    {
        FakeShipmentRepository repository = new();
        Shipment shipment = CreateShipment();
        shipment.ApplyStatusUpdate(Guid.NewGuid(), ShipmentStatus.InTransit, DateTimeOffset.UtcNow.AddMinutes(1));
        repository.Add(shipment);
        ShipmentCancellationService service = new(repository, repository, repository);

        ShipmentCancellationResult result = await service.CancelAsync(
            new ShipmentCancellationRequest(shipment.OrderId, "Order was cancelled."),
            CancellationToken.None);

        Assert.Equal(ShipmentCancellationResult.NotCancellable, result);
        Assert.Equal(ShipmentStatus.InTransit, shipment.Status);
        Assert.Equal(0, repository.SaveChangesCount);
    }

    [Fact]
    public async Task Missing_shipment_reports_not_found()
    {
        FakeShipmentRepository repository = new();
        ShipmentCancellationService service = new(repository, repository, repository);

        ShipmentCancellationResult result = await service.CancelAsync(
            new ShipmentCancellationRequest(Guid.NewGuid(), "Order was cancelled."),
            CancellationToken.None);

        Assert.Equal(ShipmentCancellationResult.NotFound, result);
    }

    [Fact]
    public void Cancelled_shipment_ignores_carrier_progress()
    {
        Shipment shipment = CreateShipment();
        shipment.Cancel("Order was cancelled.");

        ShipmentStatusUpdateResult result = shipment.ApplyStatusUpdate(
            Guid.NewGuid(),
            ShipmentStatus.InTransit,
            DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.Equal(ShipmentStatusUpdateResult.Terminal, result);
        Assert.Equal(ShipmentStatus.Cancelled, shipment.Status);
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
