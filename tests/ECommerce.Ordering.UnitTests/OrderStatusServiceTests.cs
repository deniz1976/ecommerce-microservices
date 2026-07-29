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
        OrderStatusService service = new(repository, repository);

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
        OrderStatusService service = new(repository, repository);

        await service.CancelAsync(order.Id, order.CustomerId, "INSUFFICIENT_STOCK", CancellationToken.None);

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal("INSUFFICIENT_STOCK", order.StatusHistory.Last().ReasonCode);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task ConfirmAsync_does_not_change_cancelled_order()
    {
        OrderStatusFakeOrderRepository repository = new();
        Order order = CreateOrder();
        order.MarkCancelled();
        repository.Add(order);
        OrderStatusService service = new(repository, repository);

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
        OrderStatusService service = new(repository, repository);

        await service.CancelAsync(order.Id, order.CustomerId, "provider-secret-detail", CancellationToken.None);

        Assert.Equal(OrderStatus.Confirmed, order.Status);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task ConfirmAsync_ignores_customer_mismatch()
    {
        OrderStatusFakeOrderRepository repository = new();
        Order order = CreateOrder();
        repository.Add(order);
        OrderStatusService service = new(repository, repository);

        await service.ConfirmAsync(order.Id, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(OrderStatus.Submitted, order.Status);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task WorkflowProgressAdvancesMonotonically()
    {
        OrderStatusFakeOrderRepository repository = new();
        Order order = CreateOrder();
        repository.Add(order);
        OrderStatusService service = new(repository, repository);

        await service.PaymentAuthorizedAsync(order.Id, order.CustomerId, CancellationToken.None);
        await service.InventoryReservedAsync(order.Id, order.CustomerId, CancellationToken.None);
        await service.ShipmentCreatedAsync(order.Id, order.CustomerId, CancellationToken.None);

        Assert.Equal(OrderStatus.ShipmentCreated, order.Status);
        Assert.Equal(
            [
                OrderStatus.Submitted,
                OrderStatus.InventoryReserved,
                OrderStatus.PaymentAuthorized,
                OrderStatus.ShipmentCreated
            ],
            order.StatusHistory.Select(entry => entry.Status));
        Assert.Equal(2, repository.SaveCount);
    }

    [Fact]
    public async Task CancelAsync_replaces_unknown_reason_with_customer_safe_code()
    {
        OrderStatusFakeOrderRepository repository = new();
        Order order = CreateOrder();
        repository.Add(order);
        OrderStatusService service = new(repository, repository);

        await service.CancelAsync(order.Id, order.CustomerId, "provider-secret-detail", CancellationToken.None);

        Assert.Equal("UNEXPECTED_ERROR", order.StatusHistory.Last().ReasonCode);
        Assert.All(order.StatusHistory, entry => Assert.Equal(TimeSpan.Zero, entry.OccurredAt.Offset));
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
