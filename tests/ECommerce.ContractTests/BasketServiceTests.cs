using ECommerce.Basket.Application;
using ECommerce.Basket.Application.Baskets;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.ContractTests;

public sealed class BasketServiceTests
{
    [Fact]
    public async Task AddItemUsesCanonicalCatalogProductData()
    {
        Guid customerId = Guid.NewGuid();
        Guid productId = Guid.NewGuid();
        InMemoryActiveBasketStore store = new();
        StubProductCatalogReader catalogReader = new(
            Result<CatalogProductSnapshot>.Success(
                new CatalogProductSnapshot(productId, "Canonical product", 149.90m, "TRY", 1)));
        StubBasketHistoryRepository history = new();
        BasketService service = new(store, history, history, catalogReader, new StubCheckoutPublisher());

        Result<BasketResponse> result = await service.AddItemAsync(
            customerId,
            new AddBasketItemRequest(productId, 2),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        BasketItemResponse item = Assert.Single(result.Value!.Items);
        Assert.Equal("Canonical product", item.ProductName);
        Assert.Equal(149.90m, item.UnitPrice);
        Assert.Equal(299.80m, item.TotalPrice);
        Assert.Equal("TRY", item.Currency);
        Assert.NotNull(store.SavedBasket);
    }

    [Fact]
    public async Task AddItemDoesNotPersistWhenCatalogIsUnavailable()
    {
        StubProductCatalogReader catalogReader = new(
            Result<CatalogProductSnapshot>.Failure(
                new Error(BasketErrorCodes.ProductCatalogUnavailable, BasketErrorCodes.ProductCatalogUnavailable)));
        InMemoryActiveBasketStore store = new();
        StubBasketHistoryRepository history = new();
        BasketService service = new(store, history, history, catalogReader, new StubCheckoutPublisher());

        Result<BasketResponse> result = await service.AddItemAsync(
            Guid.NewGuid(),
            new AddBasketItemRequest(Guid.NewGuid(), 1),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(BasketErrorCodes.ProductCatalogUnavailable, result.Error!.Code);
        Assert.Null(store.SavedBasket);
    }

    [Fact]
    public async Task AddItemRejectsAProductWithDifferentCurrency()
    {
        Guid customerId = Guid.NewGuid();
        Guid existingProductId = Guid.NewGuid();
        Guid newProductId = Guid.NewGuid();
        InMemoryActiveBasketStore store = new();
        ECommerce.Basket.Domain.Basket basket = new(customerId, "TRY");
        basket.AddOrUpdateItem(existingProductId, "Existing product", 1, 50m, "TRY");
        await store.SaveAsync(basket, CancellationToken.None);
        StubProductCatalogReader catalogReader = new(
            Result<CatalogProductSnapshot>.Success(
                new CatalogProductSnapshot(newProductId, "Foreign currency product", 10m, "USD", 1)));
        StubBasketHistoryRepository history = new();
        BasketService service = new(store, history, history, catalogReader, new StubCheckoutPublisher());

        Result<BasketResponse> result = await service.AddItemAsync(
            customerId,
            new AddBasketItemRequest(newProductId, 1),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(BasketErrorCodes.CurrencyMismatch, result.Error!.Code);
        Assert.Single(store.SavedBasket!.Items);
    }

    [Fact]
    public async Task CheckoutPublishesPersistedCanonicalSnapshot()
    {
        Guid customerId = Guid.NewGuid();
        Guid productId = Guid.NewGuid();
        InMemoryActiveBasketStore store = new();
        ECommerce.Basket.Domain.Basket basket = new(customerId, "TRY");
        basket.AddOrUpdateItem(productId, "Canonical product", 2, 125m, "TRY");
        await store.SaveAsync(basket, CancellationToken.None);
        StubBasketHistoryRepository history = new();
        StubCheckoutPublisher publisher = new();
        BasketService service = new(
            store,
            history,
            history,
            new StubProductCatalogReader(
                Result<CatalogProductSnapshot>.Failure(
                    new Error(ErrorCodes.UnexpectedError, ErrorCodes.UnexpectedError))),
            publisher);

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
        Result<CheckoutBasketResponse> retry = await service.CheckoutAsync(
            customerId,
            request,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(history.AddedSnapshot);
        Assert.Same(history.AddedSnapshot, publisher.PublishedSnapshot);
        Assert.Equal(result.Value!.SnapshotId, publisher.PublishedSnapshot!.Id);
        Assert.Equal("TR", publisher.PublishedSnapshot.CountryCode);
        Assert.Equal(250m, publisher.PublishedSnapshot.TotalAmount);
        Assert.Equal(result.Value.SnapshotId, retry.Value!.SnapshotId);
        Assert.Equal(1, publisher.PublishCount);
        Assert.Null(store.SavedBasket);
    }
}
