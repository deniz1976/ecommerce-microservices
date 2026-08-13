using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.UnitTests;

public sealed class OrderItemInvariantTests
{
    [Fact]
    public void AddItemRejectsMixedCurrencyWithoutChangingState()
    {
        Order order = CreateOrder();
        DateTimeOffset updatedAt = order.UpdatedAt;

        OrderItemMutationResult result = order.AddItem(
            Guid.NewGuid(),
            "Product",
            1,
            10m,
            "EUR");

        Assert.Equal(OrderItemMutationResult.CurrencyMismatch, result);
        Assert.Empty(order.Items);
        Assert.Equal("USD", order.Currency);
        Assert.Equal(updatedAt, order.UpdatedAt);
    }

    [Fact]
    public void AddItemRejectsDuplicateProductWithoutChangingState()
    {
        Order order = CreateOrder();
        Guid productId = Guid.NewGuid();
        Assert.Equal(
            OrderItemMutationResult.Applied,
            order.AddItem(productId, "Product", 1, 10m, "USD"));
        DateTimeOffset updatedAt = order.UpdatedAt;

        OrderItemMutationResult result = order.AddItem(
            productId,
            "Product",
            1,
            10m,
            "USD");

        Assert.Equal(OrderItemMutationResult.DuplicateProduct, result);
        Assert.Single(order.Items);
        Assert.Equal(updatedAt, order.UpdatedAt);
    }

    [Fact]
    public void AddItemRejectsInvalidItemWithoutChangingState()
    {
        Order order = CreateOrder();

        OrderItemMutationResult result = order.AddItem(
            Guid.Empty,
            " ",
            0,
            -1m,
            "USD");

        Assert.Equal(OrderItemMutationResult.InvalidItem, result);
        Assert.Empty(order.Items);
        Assert.Equal(0m, order.TotalAmount);
    }

    [Fact]
    public void AddItemNormalizesProductNameAndCurrency()
    {
        Order order = CreateOrder();

        OrderItemMutationResult result = order.AddItem(
            Guid.NewGuid(),
            "  Product  ",
            1,
            10m,
            " usd ");

        Assert.Equal(OrderItemMutationResult.Applied, result);
        OrderItem item = Assert.Single(order.Items);
        Assert.Equal("Product", item.ProductName);
        Assert.Equal("USD", item.Currency);
    }

    private static Order CreateOrder()
    {
        return new Order(
            Guid.NewGuid(),
            Guid.NewGuid(),
            " usd ",
            "Customer",
            "Address",
            "Istanbul",
            "TR",
            "34000");
    }
}
