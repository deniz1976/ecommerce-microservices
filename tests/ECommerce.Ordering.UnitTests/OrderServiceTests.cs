using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.BuildingBlocks.Contracts.Messaging;
using ECommerce.BuildingBlocks.Contracts.Orders;
using ECommerce.Ordering.Application.Orders;
using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.UnitTests;

public sealed class OrderServiceTests
{
    [Fact]
    public async Task CreateAsync_returns_validation_error_when_customer_id_is_empty()
    {
        OrderServiceFakeOrderRepository repository = new();
        FakeOrderSubmittedPublisher publisher = new(repository);
        OrderService service = new(repository, repository, publisher);

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
        OrderServiceFakeOrderRepository repository = new();
        FakeOrderSubmittedPublisher publisher = new(repository);
        OrderService service = new(repository, repository, publisher);

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
        OrderServiceFakeOrderRepository repository = new();
        FakeOrderSubmittedPublisher publisher = new(repository);
        OrderService service = new(repository, repository, publisher);

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

    [Fact]
    public async Task CreateFromCheckoutAsync_is_idempotent_and_uses_checkout_id()
    {
        OrderServiceFakeOrderRepository repository = new();
        FakeOrderSubmittedPublisher publisher = new(repository);
        OrderService service = new(repository, repository, publisher);
        Guid checkoutId = Guid.NewGuid();
        Guid storeId = Guid.NewGuid();
        BasketCheckedOut checkout = new(
            Guid.NewGuid(),
            checkoutId,
            null,
            DateTimeOffset.UtcNow,
            MessageDefaults.CurrentVersion,
            checkoutId,
            Guid.NewGuid(),
            25m,
            "USD",
            "Test Customer",
            "Address 1",
            "Istanbul",
            "TR",
            "34000",
            [new OrderLine(Guid.NewGuid(), "Test Product", 2, 12.50m, "USD", storeId)]);

        var first = await service.CreateFromCheckoutAsync(checkout, CancellationToken.None);
        var duplicate = await service.CreateFromCheckoutAsync(checkout, CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(duplicate.IsSuccess);
        Assert.Equal(checkoutId, first.Value!.Id);
        Assert.Single(repository.Orders);
        Assert.Equal(storeId, Assert.Single(repository.Orders[0].Items).StoreId);
        Assert.Equal(1, publisher.PublishCount);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task DirectCreateDoesNotAcceptSellerStoreAttribution()
    {
        OrderServiceFakeOrderRepository repository = new();
        FakeOrderSubmittedPublisher publisher = new(repository);
        OrderService service = new(repository, repository, publisher);

        var result = await service.CreateAsync(
            CreateValidRequest(),
            Guid.NewGuid(),
            null,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(Assert.Single(repository.Orders[0].Items).StoreId);
    }

    [Fact]
    public async Task CreateAsyncRejectsDuplicateProductsBeforePublishing()
    {
        OrderServiceFakeOrderRepository repository = new();
        FakeOrderSubmittedPublisher publisher = new(repository);
        OrderService service = new(repository, repository, publisher);
        CreateOrderRequest validRequest = CreateValidRequest();
        CreateOrderItemRequest item = Assert.Single(validRequest.Items);
        CreateOrderRequest request = validRequest with { Items = [item, item] };

        var result = await service.CreateAsync(
            request,
            Guid.NewGuid(),
            null,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Empty(repository.Orders);
        Assert.Equal(0, publisher.PublishCount);
        Assert.Equal(0, repository.SaveCount);
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
}
