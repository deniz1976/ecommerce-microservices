using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.Ordering.Application.Orders;
using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.UnitTests;

public sealed class OrderServiceTests
{
    [Fact]
    public async Task CreateAsync_returns_validation_error_when_customer_id_is_empty()
    {
        FakeOrderRepository repository = new();
        FakeOrderSubmittedPublisher publisher = new(repository);
        OrderService service = new(repository, publisher);

        CreateOrderRequest request = CreateValidRequest() with { CustomerId = Guid.Empty };

        var result = await service.CreateAsync(request, Guid.NewGuid(), null, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.ValidationFailed, result.Error!.Code);
        Assert.Empty(repository.Orders);
        Assert.Equal(0, publisher.PublishCount);
    }

    [Fact]
    public async Task CreateAsync_trims_shipping_fields_and_uppercases_country_code()
    {
        FakeOrderRepository repository = new();
        FakeOrderSubmittedPublisher publisher = new(repository);
        OrderService service = new(repository, publisher);

        CreateOrderRequest request = CreateValidRequest() with
        {
            RecipientName = " Test Customer ",
            AddressLine = " Address 1 ",
            City = " Istanbul ",
            CountryCode = " tr ",
            PostalCode = " 34000 "
        };

        var result = await service.CreateAsync(request, Guid.NewGuid(), null, CancellationToken.None);

        Assert.False(result.IsFailure);
        Order order = Assert.Single(repository.Orders);
        Assert.Equal("Test Customer", order.RecipientName);
        Assert.Equal("Address 1", order.AddressLine);
        Assert.Equal("Istanbul", order.City);
        Assert.Equal("TR", order.CountryCode);
        Assert.Equal("34000", order.PostalCode);
    }

    [Fact]
    public async Task CreateAsync_publishes_before_saving_order()
    {
        FakeOrderRepository repository = new();
        FakeOrderSubmittedPublisher publisher = new(repository);
        OrderService service = new(repository, publisher);

        Guid correlationId = Guid.NewGuid();
        Guid causationId = Guid.NewGuid();

        var result = await service.CreateAsync(CreateValidRequest(), correlationId, causationId, CancellationToken.None);

        Assert.False(result.IsFailure);
        Assert.Equal(1, publisher.PublishCount);
        Assert.Equal(correlationId, publisher.CorrelationId);
        Assert.Equal(causationId, publisher.CausationId);
        Assert.True(publisher.WasPublishedBeforeSave);
        Assert.Equal(1, repository.SaveCount);
    }

    private static CreateOrderRequest CreateValidRequest()
    {
        return new CreateOrderRequest(
            Guid.NewGuid(),
            "USD",
            "Test Customer",
            "Address 1",
            "Istanbul",
            "TR",
            "34000",
            [new CreateOrderItemRequest(Guid.NewGuid(), "Test Product", 2, 12.50m, "USD")]);
    }

    private sealed class FakeOrderRepository : IOrderRepository
    {
        public List<Order> Orders { get; } = [];

        public int SaveCount { get; private set; }

        public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Orders.FirstOrDefault(x => x.Id == id));
        }

        public Task<IReadOnlyCollection<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken)
        {
            IReadOnlyCollection<Order> orders = Orders.Where(x => x.CustomerId == customerId).ToArray();
            return Task.FromResult(orders);
        }

        public void Add(Order order)
        {
            Orders.Add(order);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeOrderSubmittedPublisher : IOrderSubmittedPublisher
    {
        private readonly FakeOrderRepository repository;

        public FakeOrderSubmittedPublisher(FakeOrderRepository repository)
        {
            this.repository = repository;
        }

        public int PublishCount { get; private set; }

        public Guid? CorrelationId { get; private set; }

        public Guid? CausationId { get; private set; }

        public bool WasPublishedBeforeSave { get; private set; }

        public Task PublishAsync(Order order, Guid correlationId, Guid? causationId, CancellationToken cancellationToken)
        {
            PublishCount++;
            CorrelationId = correlationId;
            CausationId = causationId;
            WasPublishedBeforeSave = repository.SaveCount == 0;
            return Task.CompletedTask;
        }
    }
}
