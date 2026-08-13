using ECommerce.Basket.Domain;
using BasketEntity = ECommerce.Basket.Domain.Basket;

namespace ECommerce.ContractTests;

public sealed class BasketTimestampTests
{
    [Fact]
    public void RemovingMissingItemDoesNotAdvanceMutationInstant()
    {
        DateTimeOffset updatedAt = new(2026, 8, 13, 12, 0, 0, TimeSpan.Zero);
        BasketEntity basket = new(
            Guid.NewGuid(),
            "TRY",
            updatedAt.AddHours(-1),
            updatedAt,
            Array.Empty<BasketItem>());

        basket.RemoveItem(Guid.NewGuid());

        Assert.Equal(updatedAt, basket.UpdatedAt);
    }

    [Fact]
    public void RestoredBasketPreservesInstantsAndNormalizesOffsetsToUtc()
    {
        DateTimeOffset createdAt = new(2026, 7, 25, 12, 0, 0, TimeSpan.FromHours(3));
        DateTimeOffset updatedAt = createdAt.AddMinutes(15);
        BasketItem[] items =
        [
            new BasketItem(Guid.NewGuid(), "Product", 1, 25m, "TRY")
        ];

        BasketEntity basket = new(Guid.NewGuid(), "TRY", createdAt, updatedAt, items);

        Assert.Equal(TimeSpan.Zero, basket.CreatedAt.Offset);
        Assert.Equal(TimeSpan.Zero, basket.UpdatedAt.Offset);
        Assert.Equal(createdAt.UtcTicks, basket.CreatedAt.UtcTicks);
        Assert.Equal(updatedAt.UtcTicks, basket.UpdatedAt.UtcTicks);
        Assert.Single(basket.Items);
    }
}
