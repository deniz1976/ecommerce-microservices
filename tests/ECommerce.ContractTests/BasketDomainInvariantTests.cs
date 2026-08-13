using ECommerce.Basket.Domain;
using BasketEntity = ECommerce.Basket.Domain.Basket;

namespace ECommerce.ContractTests;

public sealed class BasketDomainInvariantTests
{
    [Fact]
    public void AddOrUpdateItemRejectsInvalidItemWithoutChangingState()
    {
        BasketEntity basket = new(Guid.NewGuid(), "TRY");

        BasketItemMutationResult result = basket.AddOrUpdateItem(
            Guid.NewGuid(),
            "Product",
            0,
            10m,
            "TRY");

        Assert.Equal(BasketItemMutationResult.InvalidItem, result);
        Assert.Empty(basket.Items);
        Assert.Equal(0m, basket.TotalAmount);
    }

    [Fact]
    public void AddOrUpdateItemRejectsMixedCurrencyWithoutChangingState()
    {
        BasketEntity basket = new(Guid.NewGuid(), "TRY");
        Assert.Equal(
            BasketItemMutationResult.Applied,
            basket.AddOrUpdateItem(Guid.NewGuid(), "First", 1, 10m, "TRY"));
        DateTimeOffset updatedAt = basket.UpdatedAt;

        BasketItemMutationResult result = basket.AddOrUpdateItem(
            Guid.NewGuid(),
            "Second",
            1,
            20m,
            "USD");

        Assert.Equal(BasketItemMutationResult.CurrencyMismatch, result);
        Assert.Single(basket.Items);
        Assert.Equal("TRY", basket.Currency);
        Assert.Equal(updatedAt, basket.UpdatedAt);
    }
}
