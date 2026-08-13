using Microsoft.Extensions.Options;
using BasketEntity = ECommerce.Basket.Domain.Basket;

namespace ECommerce.Basket.Infrastructure.Redis;

public sealed class BasketExpirationPolicy
{
    private readonly TimeProvider timeProvider;
    private readonly TimeSpan lifetime;

    public BasketExpirationPolicy(IOptions<RedisOptions> options, TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;
        lifetime = TimeSpan.FromHours(options.Value.BasketTtlHours);
    }

    public TimeSpan? GetRemainingLifetime(BasketEntity basket)
    {
        DateTimeOffset expiresAt = basket.UpdatedAt.ToUniversalTime().Add(lifetime);
        TimeSpan remainingLifetime = expiresAt - timeProvider.GetUtcNow();

        return remainingLifetime > TimeSpan.Zero ? remainingLifetime : null;
    }
}
