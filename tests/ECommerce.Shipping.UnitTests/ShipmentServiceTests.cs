using ECommerce.Shipping.Application.Shipments;
using ECommerce.Shipping.Domain;

namespace ECommerce.Shipping.UnitTests;

public sealed class ShipmentServiceTests
{
    [Fact]
    public async Task CreateAsyncPersistsCreatedShipmentWhenProviderAcceptsRequest()
    {
        FakeShipmentRepository repository = new();
        StubShippingProvider provider = new(new ShippingProviderResult(true, "TRACK-123", null));
        ShipmentService service = new(repository, provider);

        CreateShipmentResult result = await service.CreateAsync(CreateValidRequest(), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("TRACK-123", result.TrackingNumber);
        Assert.Equal(ShipmentStatus.Created, repository.Shipment!.Status);
        Assert.Equal(1, repository.SaveChangesCount);
    }

    [Fact]
    public async Task CreateAsyncPersistsFailedShipmentWhenProviderRejectsRequest()
    {
        FakeShipmentRepository repository = new();
        StubShippingProvider provider = new(new ShippingProviderResult(false, null, "Postal code rejected."));
        ShipmentService service = new(repository, provider);

        CreateShipmentResult result = await service.CreateAsync(CreateValidRequest(), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("Postal code rejected.", result.Reason);
        Assert.Equal(ShipmentStatus.Failed, repository.Shipment!.Status);
        Assert.Null(repository.Shipment.TrackingNumber);
        Assert.Equal("Postal code rejected.", repository.Shipment.FailureReason);
        Assert.Equal(1, repository.SaveChangesCount);
    }

    [Fact]
    public async Task CreateAsyncDoesNotCallProviderWhenAddressIsInvalid()
    {
        FakeShipmentRepository repository = new();
        StubShippingProvider provider = new(new ShippingProviderResult(true, "TRACK-123", null));
        ShipmentService service = new(repository, provider);
        CreateShipmentRequest request = CreateValidRequest() with { PostalCode = " " };

        CreateShipmentResult result = await service.CreateAsync(request, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(0, provider.CallCount);
        Assert.Equal(ShipmentStatus.Failed, repository.Shipment!.Status);
        Assert.Null(repository.Shipment.TrackingNumber);
    }

    private static CreateShipmentRequest CreateValidRequest()
    {
        return new CreateShipmentRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Customer",
            "Runtime Avenue 1",
            "Istanbul",
            "TR",
            "34000");
    }

}
