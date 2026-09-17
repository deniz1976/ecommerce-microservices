using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.UnitTests;

public sealed class OrderCancellationRejectionTests
{
    [Fact]
    public void RejectedCancellationKeepsOrderOnItsWorkflowProgress()
    {
        Order order = CreateOrder();
        order.MarkInventoryReserved();
        order.MarkCancellationRequested();

        order.MarkCancellationRejected();

        Assert.Equal(OrderStatus.InventoryReserved, order.Status);
    }

    [Fact]
    public void RejectedCancellationStillAllowsRemainingWorkflowEvents()
    {
        Order order = CreateOrder();
        order.MarkInventoryReserved();
        order.MarkCancellationRequested();
        order.MarkCancellationRejected();

        order.MarkPaymentAuthorized();
        order.MarkShipmentCreated();
        order.MarkConfirmed();

        Assert.Equal(OrderStatus.Confirmed, order.Status);
        Assert.Contains(order.StatusHistory, entry => entry.Status == OrderStatus.PaymentAuthorized);
        Assert.Contains(order.StatusHistory, entry => entry.Status == OrderStatus.ShipmentCreated);
    }

    private static Order CreateOrder() =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "USD",
            "Customer",
            "Address",
            "Istanbul",
            "TR",
            "34000");
}
