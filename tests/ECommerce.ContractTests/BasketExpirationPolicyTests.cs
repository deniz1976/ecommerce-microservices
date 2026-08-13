using ECommerce.Basket.Domain;
using ECommerce.Basket.Infrastructure.Redis;
using Microsoft.Extensions.Options;
using BasketEntity = ECommerce.Basket.Domain.Basket;

namespace ECommerce.ContractTests;

public sealed class BasketExpirationPolicyTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(72, true)]
    [InlineData(720, true)]
    [InlineData(721, false)]
    public void ConfiguredLifetimeIsBounded(int ttlHours, bool expected)
    {
        Assert.Equal(expected, RedisOptions.IsBasketTtlValid(ttlHours));
    }

    [Fact]
    public void RemainingLifetimeIsAnchoredToLastMutationInstant()
    {
        DateTimeOffset now = new(2026, 8, 13, 15, 0, 0, TimeSpan.Zero);
        BasketEntity basket = new(
            Guid.NewGuid(),
            "TRY",
            now.AddDays(-2),
            now.AddHours(-1),
            Array.Empty<BasketItem>());
        BasketExpirationPolicy policy = CreatePolicy(72, now);

        TimeSpan? remainingLifetime = policy.GetRemainingLifetime(basket);

        Assert.Equal(TimeSpan.FromHours(71), remainingLifetime);
    }

    [Fact]
    public void ElapsedLifetimeCannotBeRenewedByPersistence()
    {
        DateTimeOffset now = new(2026, 8, 13, 15, 0, 0, TimeSpan.Zero);
        BasketEntity basket = new(
            Guid.NewGuid(),
            "TRY",
            now.AddDays(-5),
            now.AddHours(-73),
            Array.Empty<BasketItem>());
        BasketExpirationPolicy policy = CreatePolicy(72, now);

        TimeSpan? remainingLifetime = policy.GetRemainingLifetime(basket);

        Assert.Null(remainingLifetime);
    }

    private static BasketExpirationPolicy CreatePolicy(int ttlHours, DateTimeOffset now)
    {
        return new BasketExpirationPolicy(
            Options.Create(new RedisOptions { BasketTtlHours = ttlHours }),
            new FixedTimeProvider(now));
    }
}
