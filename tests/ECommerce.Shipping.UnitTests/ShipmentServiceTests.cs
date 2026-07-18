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

    private sealed class FakeShipmentRepository : IShipmentRepository
    {
        public Shipment? Shipment { get; private set; }

        public int SaveChangesCount { get; private set; }

        public Task<Shipment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Shipment?.OrderId == orderId ? Shipment : null);
        }

        public void Add(Shipment shipment)
        {
            Shipment = shipment;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class StubShippingProvider : IShippingProvider
    {
        private readonly ShippingProviderResult result;

        public StubShippingProvider(ShippingProviderResult result)
        {
            this.result = result;
        }

        public int CallCount { get; private set; }

        public Task<ShippingProviderResult> CreateAsync(
            ShippingProviderRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(result);
        }
    }
}
