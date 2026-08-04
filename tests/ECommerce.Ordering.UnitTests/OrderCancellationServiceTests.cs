using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.Ordering.Application.Orders;
using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.UnitTests;

public sealed class OrderCancellationServiceTests
{
    [Fact]
    public async Task RequestAsync_marks_order_pending_and_publishes_with_same_commit()
    {
        OrderServiceFakeOrderRepository repository = new();
        Order order = CreateOrder();
        repository.Add(order);
        FakeOrderCancellationRequestedPublisher publisher = new(repository);
        OrderCancellationService service = new(repository, repository, publisher);

        var result = await service.RequestAsync(
            order.Id,
            Guid.NewGuid(),
            null,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.CancellationRequested, order.Status);
        Assert.Equal(1, publisher.PublishCount);
        Assert.True(publisher.WasPublishedBeforeSave);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task RequestAsync_is_idempotent_while_cancellation_is_pending()
    {
        OrderServiceFakeOrderRepository repository = new();
        Order order = CreateOrder();
        order.MarkCancellationRequested();
        repository.Add(order);
        FakeOrderCancellationRequestedPublisher publisher = new(repository);
        OrderCancellationService service = new(repository, repository, publisher);

        var result = await service.RequestAsync(
            order.Id,
            Guid.NewGuid(),
            null,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, publisher.PublishCount);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task RequestAsync_rejects_confirmed_order()
    {
        OrderServiceFakeOrderRepository repository = new();
        Order order = CreateOrder();
        order.MarkConfirmed();
        repository.Add(order);
        FakeOrderCancellationRequestedPublisher publisher = new(repository);
        OrderCancellationService service = new(repository, repository, publisher);

        var result = await service.RequestAsync(
            order.Id,
            Guid.NewGuid(),
            null,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.OrderNotCancellable, result.Error!.Code);
        Assert.Equal(0, publisher.PublishCount);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task RequestAsync_rejects_order_after_payment_authorization()
    {
        OrderServiceFakeOrderRepository repository = new();
        Order order = CreateOrder();
        order.MarkInventoryReserved();
        order.MarkPaymentAuthorized();
        repository.Add(order);
        FakeOrderCancellationRequestedPublisher publisher = new(repository);
        OrderCancellationService service = new(repository, repository, publisher);

        var result = await service.RequestAsync(
            order.Id,
            Guid.NewGuid(),
            null,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.OrderNotCancellable, result.Error!.Code);
        Assert.Equal(0, publisher.PublishCount);
        Assert.Equal(0, repository.SaveCount);
    }

    private static Order CreateOrder()
    {
        Order order = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TRY",
            "Test Customer",
            "Address",
            "Istanbul",
            "TR",
            "34000");
        order.AddItem(Guid.NewGuid(), "Product", 1, 10m, "TRY");
        return order;
    }
}
