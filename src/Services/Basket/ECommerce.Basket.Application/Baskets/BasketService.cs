using ECommerce.Basket.Domain;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using BasketEntity = ECommerce.Basket.Domain.Basket;

namespace ECommerce.Basket.Application.Baskets;

public sealed class BasketService
{
    private readonly IActiveBasketStore activeBasketStore;
    private readonly IBasketHistoryRepository basketHistoryRepository;

    public BasketService(IActiveBasketStore activeBasketStore, IBasketHistoryRepository basketHistoryRepository)
    {
        this.activeBasketStore = activeBasketStore;
        this.basketHistoryRepository = basketHistoryRepository;
    }

    public async Task<Result<BasketResponse>> GetAsync(Guid customerId, CancellationToken cancellationToken)
    {
        BasketEntity? basket = await activeBasketStore.GetAsync(customerId, cancellationToken);

        return basket is null
            ? Result<BasketResponse>.Failure(new Error(ErrorCodes.BasketNotFound, ErrorCodes.BasketNotFound))
            : Result<BasketResponse>.Success(basket.ToResponse());
    }

    public async Task<Result<BasketResponse>> AddItemAsync(Guid customerId, AddBasketItemRequest request, CancellationToken cancellationToken)
    {
        if (request.ProductId == Guid.Empty || request.Quantity <= 0 || request.UnitPrice < 0 || string.IsNullOrWhiteSpace(request.Currency))
        {
            return Result<BasketResponse>.Failure(new Error(BasketErrorCodes.InvalidBasketItem, BasketErrorCodes.InvalidBasketItem));
        }

        BasketEntity basket = await activeBasketStore.GetAsync(customerId, cancellationToken) ?? new BasketEntity(customerId, request.Currency);
        basket.AddOrUpdateItem(request.ProductId, request.ProductName, request.Quantity, request.UnitPrice, request.Currency);

        await activeBasketStore.SaveAsync(basket, cancellationToken);

        return Result<BasketResponse>.Success(basket.ToResponse());
    }

    public async Task<Result<BasketResponse>> RemoveItemAsync(Guid customerId, Guid productId, CancellationToken cancellationToken)
    {
        BasketEntity? basket = await activeBasketStore.GetAsync(customerId, cancellationToken);

        if (basket is null)
        {
            return Result<BasketResponse>.Failure(new Error(ErrorCodes.BasketNotFound, ErrorCodes.BasketNotFound));
        }

        basket.RemoveItem(productId);
        await activeBasketStore.SaveAsync(basket, cancellationToken);

        return Result<BasketResponse>.Success(basket.ToResponse());
    }

    public async Task<Result> ClearAsync(Guid customerId, CancellationToken cancellationToken)
    {
        await activeBasketStore.DeleteAsync(customerId, cancellationToken);
        return Result.Success();
    }

    public async Task<Result<CheckoutBasketResponse>> CheckoutAsync(Guid customerId, CancellationToken cancellationToken)
    {
        BasketEntity? basket = await activeBasketStore.GetAsync(customerId, cancellationToken);

        if (basket is null)
        {
            return Result<CheckoutBasketResponse>.Failure(new Error(ErrorCodes.BasketNotFound, ErrorCodes.BasketNotFound));
        }

        if (basket.Items.Count == 0)
        {
            return Result<CheckoutBasketResponse>.Failure(new Error(BasketErrorCodes.EmptyBasket, BasketErrorCodes.EmptyBasket));
        }

        BasketCheckoutSnapshot snapshot = new(Guid.NewGuid(), basket.CustomerId, basket.Currency, basket.TotalAmount);

        foreach (BasketItem item in basket.Items)
        {
            snapshot.AddItem(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice, item.Currency);
        }

        basketHistoryRepository.Add(snapshot);
        await basketHistoryRepository.SaveChangesAsync(cancellationToken);
        await activeBasketStore.DeleteAsync(customerId, cancellationToken);

        return Result<CheckoutBasketResponse>.Success(new CheckoutBasketResponse(snapshot.Id, snapshot.CustomerId, snapshot.TotalAmount, snapshot.Currency, snapshot.CreatedAt));
    }
}
