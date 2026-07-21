using ECommerce.Ordering.Application.Orders;
using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.UnitTests;

public sealed class OrderStatusServiceTests
{
    [Fact]
    public async Task ConfirmAsync_marks_submitted_order_as_confirmed()
    {
        OrderStatusFakeOrderRepository repository = new();
        Order order = CreateOrder();
        repository.Add(order);
        OrderStatusService service = new(repository);

        await service.ConfirmAsync(order.Id, order.CustomerId, CancellationToken.None);

        Assert.Equal(OrderStatus.Confirmed, order.Status);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task CancelAsync_marks_submitted_order_as_cancelled()
    {
        OrderStatusFakeOrderRepository repository = new();
        Order order = CreateOrder();
        repository.Add(order);
        OrderStatusService service = new(repository);

        await service.CancelAsync(order.Id, order.CustomerId, CancellationToken.None);

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task ConfirmAsync_does_not_change_cancelled_order()
    {
        OrderStatusFakeOrderRepository repository = new();
        Order order = CreateOrder();
        order.MarkCancelled();
        repository.Add(order);
        OrderStatusService service = new(repository);

        await service.ConfirmAsync(order.Id, order.CustomerId, CancellationToken.None);

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task CancelAsync_does_not_change_confirmed_order()
    {
        OrderStatusFakeOrderRepository repository = new();
        Order order = CreateOrder();
        order.MarkConfirmed();
        repository.Add(order);
        OrderStatusService service = new(repository);

        await service.CancelAsync(order.Id, order.CustomerId, CancellationToken.None);

        Assert.Equal(OrderStatus.Confirmed, order.Status);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task ConfirmAsync_ignores_customer_mismatch()
    {
        OrderStatusFakeOrderRepository repository = new();
        Order order = CreateOrder();
        repository.Add(order);
        OrderStatusService service = new(repository);

        await service.ConfirmAsync(order.Id, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(OrderStatus.Submitted, order.Status);
        Assert.Equal(0, repository.SaveCount);
    }

    private static Order CreateOrder()
    {
        Order order = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "USD",
            "Test Customer",
            "Address 1",
            "Istanbul",
            "TR",
            "34000");

        order.AddItem(Guid.NewGuid(), "Test Product", 1, 10m, "USD");
        return order;
    }

}
