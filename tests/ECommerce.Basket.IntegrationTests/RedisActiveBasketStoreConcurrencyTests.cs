using ECommerce.Basket.Application;
using ECommerce.Basket.Infrastructure.Redis;
using ECommerce.BuildingBlocks.Contracts.Results;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using BasketEntity = ECommerce.Basket.Domain.Basket;

namespace ECommerce.Basket.IntegrationTests;

[Collection(RedisCollection.Name)]
public sealed class RedisActiveBasketStoreConcurrencyTests(RedisFixture fixture)
{
    [Fact]
    public async Task Concurrent_basket_updates_reject_the_stale_writer()
    {
        Guid customerId = Guid.NewGuid();
        RedisActiveBasketStore setupStore = CreateStore();
        BasketEntity initial = new(customerId, "TRY");
        initial.AddOrUpdateItem(Guid.NewGuid(), "First product", 1, 10m, "TRY");
        Assert.True((await setupStore.SaveAsync(initial, CancellationToken.None)).IsSuccess);

        RedisActiveBasketStore firstStore = CreateStore();
        RedisActiveBasketStore staleStore = CreateStore();
        BasketEntity firstBasket = (await firstStore.GetAsync(customerId, CancellationToken.None)).Value!;
        BasketEntity staleBasket = (await staleStore.GetAsync(customerId, CancellationToken.None)).Value!;
        firstBasket.AddOrUpdateItem(Guid.NewGuid(), "Second product", 1, 20m, "TRY");
        staleBasket.AddOrUpdateItem(Guid.NewGuid(), "Third product", 1, 30m, "TRY");

        Result firstResult = await firstStore.SaveAsync(firstBasket, CancellationToken.None);
        Result staleResult = await staleStore.SaveAsync(staleBasket, CancellationToken.None);

        Assert.True(firstResult.IsSuccess);
        Assert.True(staleResult.IsFailure);
        Assert.Equal(BasketErrorCodes.BasketConcurrentUpdate, staleResult.Error!.Code);
        BasketEntity persisted = (await CreateStore().GetAsync(customerId, CancellationToken.None)).Value!;
        Assert.Equal(30m, persisted.TotalAmount);
    }

    [Fact]
    public async Task Concurrent_first_baskets_reject_the_second_creator()
    {
        Guid customerId = Guid.NewGuid();
        RedisActiveBasketStore firstStore = CreateStore();
        RedisActiveBasketStore staleStore = CreateStore();
        Assert.Null((await firstStore.GetAsync(customerId, CancellationToken.None)).Value);
        Assert.Null((await staleStore.GetAsync(customerId, CancellationToken.None)).Value);
        BasketEntity firstBasket = new(customerId, "TRY");
        firstBasket.AddOrUpdateItem(Guid.NewGuid(), "First product", 1, 10m, "TRY");
        BasketEntity staleBasket = new(customerId, "TRY");
        staleBasket.AddOrUpdateItem(Guid.NewGuid(), "Second product", 1, 20m, "TRY");

        Assert.True((await firstStore.SaveAsync(firstBasket, CancellationToken.None)).IsSuccess);
        Result staleResult = await staleStore.SaveAsync(staleBasket, CancellationToken.None);

        Assert.Equal(BasketErrorCodes.BasketConcurrentUpdate, staleResult.Error!.Code);
    }

    [Fact]
    public async Task Sequential_updates_in_one_scope_keep_succeeding()
    {
        Guid customerId = Guid.NewGuid();
        RedisActiveBasketStore store = CreateStore();
        Assert.Null((await store.GetAsync(customerId, CancellationToken.None)).Value);
        BasketEntity basket = new(customerId, "TRY");
        basket.AddOrUpdateItem(Guid.NewGuid(), "First product", 1, 10m, "TRY");

        Assert.True((await store.SaveAsync(basket, CancellationToken.None)).IsSuccess);
        basket.AddOrUpdateItem(Guid.NewGuid(), "Second product", 1, 20m, "TRY");
        Assert.True((await store.SaveAsync(basket, CancellationToken.None)).IsSuccess);
    }

    private RedisActiveBasketStore CreateStore()
    {
        IOptions<RedisOptions> options = Options.Create(new RedisOptions { BasketTtlHours = 1 });
        return new RedisActiveBasketStore(
            fixture.Connection,
            new BasketExpirationPolicy(options, TimeProvider.System),
            new BasketStoreMetrics($"ECommerce.Basket.IntegrationTests.{Guid.NewGuid():N}", TimeSpan.FromHours(1)),
            NullLogger<RedisActiveBasketStore>.Instance);
    }
}
