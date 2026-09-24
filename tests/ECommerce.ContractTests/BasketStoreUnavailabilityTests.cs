using ECommerce.Basket.Application;
using ECommerce.Basket.Application.Baskets;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.ContractTests;

public sealed class BasketStoreUnavailabilityTests
{
    [Fact]
    public async Task ReadReportsStoreUnavailabilityInsteadOfAnEmptyBasket()
    {
        InMemoryActiveBasketStore store = new() { IsUnavailable = true };
        BasketQueryService service = new(store);

        Result<BasketResponse> result = await service.GetAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(BasketErrorCodes.BasketStoreUnavailable, result.Error!.Code);
    }

    [Fact]
    public async Task AddItemReportsStoreUnavailabilityWithoutAcceptingTheItem()
    {
        Guid productId = Guid.NewGuid();
        InMemoryActiveBasketStore store = new() { IsUnavailable = true };
        StubProductCatalogReader catalogReader = new(
            Result<CatalogProductSnapshot>.Success(
                new CatalogProductSnapshot(productId, "Canonical product", 10m, "TRY", 1)));
        BasketItemAdditionService service = new(store, catalogReader);

        Result<BasketResponse> result = await service.AddAsync(
            Guid.NewGuid(),
            new AddBasketItemRequest(productId, 1),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(BasketErrorCodes.BasketStoreUnavailable, result.Error!.Code);
        Assert.Null(store.SavedBasket);
    }

    [Fact]
    public async Task ClearReportsStoreUnavailability()
    {
        InMemoryActiveBasketStore store = new() { IsUnavailable = true };
        BasketClearService service = new(store);

        Result result = await service.ClearAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(BasketErrorCodes.BasketStoreUnavailable, result.Error!.Code);
    }

    [Fact]
    public async Task CheckoutSucceedsWhenTheStoreFailsAfterTheSnapshotIsCommitted()
    {
        Guid customerId = Guid.NewGuid();
        InMemoryActiveBasketStore store = new();
        ECommerce.Basket.Domain.Basket basket = new(customerId, "TRY");
        basket.AddOrUpdateItem(Guid.NewGuid(), "Canonical product", 1, 100m, "TRY");
        await store.SaveAsync(basket, CancellationToken.None);
        StubBasketHistoryRepository history = new();
        StubCheckoutPublisher publisher = new();
        UnavailableAfterCommitUnitOfWork unitOfWork = new(history, store);
        BasketCheckoutService service = new(
            store,
            history,
            unitOfWork,
            publisher,
            new BasketCatalogRevalidationService(store, InMemoryProductCatalogReader.Matching(basket)));

        CheckoutBasketRequest request = new(
            Guid.NewGuid(),
            "Deniz Test",
            "Test Street 1",
            "Istanbul",
            "tr",
            "34000");
        Result<CheckoutBasketResponse> result = await service.CheckoutAsync(
            customerId,
            request,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(publisher.PublishedSnapshot);
        Assert.Equal(1, store.DeleteAttemptCount);
    }

    [Fact]
    public async Task CheckoutReportsStoreUnavailabilityBeforeCommittingASnapshot()
    {
        InMemoryActiveBasketStore store = new() { IsUnavailable = true };
        StubBasketHistoryRepository history = new();
        StubCheckoutPublisher publisher = new();
        BasketCheckoutService service = new(
            store,
            history,
            history,
            publisher,
            new BasketCatalogRevalidationService(store, new InMemoryProductCatalogReader()));

        Result<CheckoutBasketResponse> result = await service.CheckoutAsync(
            Guid.NewGuid(),
            new CheckoutBasketRequest(
                Guid.NewGuid(),
                "Deniz Test",
                "Test Street 1",
                "Istanbul",
                "tr",
                "34000"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(BasketErrorCodes.BasketStoreUnavailable, result.Error!.Code);
        Assert.Null(history.AddedSnapshot);
        Assert.Null(publisher.PublishedSnapshot);
    }
}
