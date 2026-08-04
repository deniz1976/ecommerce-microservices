using ECommerce.Basket.Domain;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using BasketEntity = ECommerce.Basket.Domain.Basket;

namespace ECommerce.Basket.Application.Baskets;

public sealed class BasketService
{
    private readonly IActiveBasketStore activeBasketStore;
    private readonly IRepository<BasketCheckoutSnapshot, Guid> basketHistoryRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IProductCatalogReader productCatalogReader;
    private readonly ICheckoutPublisher checkoutPublisher;

    public BasketService(
        IActiveBasketStore activeBasketStore,
        IRepository<BasketCheckoutSnapshot, Guid> basketHistoryRepository,
        IUnitOfWork unitOfWork,
        IProductCatalogReader productCatalogReader,
        ICheckoutPublisher checkoutPublisher)
    {
        this.activeBasketStore = activeBasketStore;
        this.basketHistoryRepository = basketHistoryRepository;
        this.unitOfWork = unitOfWork;
        this.productCatalogReader = productCatalogReader;
        this.checkoutPublisher = checkoutPublisher;
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
        if (request.ProductId == Guid.Empty || request.Quantity <= 0)
        {
            return Result<BasketResponse>.Failure(new Error(BasketErrorCodes.InvalidBasketItem, BasketErrorCodes.InvalidBasketItem));
        }

        Result<CatalogProductSnapshot> productResult = await productCatalogReader.GetActiveProductAsync(request.ProductId, cancellationToken);

        if (productResult.IsFailure)
        {
            return Result<BasketResponse>.Failure(productResult.Error!);
        }

        CatalogProductSnapshot product = productResult.Value!;
        BasketEntity basket = await activeBasketStore.GetAsync(customerId, cancellationToken) ?? new BasketEntity(customerId, product.Currency);

        if (basket.Items.Count > 0 && !string.Equals(basket.Currency, product.Currency, StringComparison.OrdinalIgnoreCase))
        {
            return Result<BasketResponse>.Failure(new Error(BasketErrorCodes.CurrencyMismatch, BasketErrorCodes.CurrencyMismatch));
        }

        basket.AddOrUpdateItem(
            product.Id,
            product.Name,
            request.Quantity,
            product.Price,
            product.Currency,
            product.StoreId);

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

    public async Task<Result<CheckoutBasketResponse>> CheckoutAsync(
        Guid customerId,
        CheckoutBasketRequest request,
        CancellationToken cancellationToken)
    {
        if (request.CheckoutId == Guid.Empty ||
            string.IsNullOrWhiteSpace(request.RecipientName) ||
            string.IsNullOrWhiteSpace(request.AddressLine) ||
            string.IsNullOrWhiteSpace(request.City) ||
            request.CountryCode.Trim().Length != 2 ||
            string.IsNullOrWhiteSpace(request.PostalCode))
        {
            return Result<CheckoutBasketResponse>.Failure(
                new Error(BasketErrorCodes.InvalidCheckoutAddress, BasketErrorCodes.InvalidCheckoutAddress));
        }

        BasketCheckoutSnapshot? existingSnapshot = await basketHistoryRepository.GetByIdAsync(
            request.CheckoutId,
            cancellationToken);
        if (existingSnapshot is not null)
        {
            if (existingSnapshot.CustomerId != customerId)
            {
                return Result<CheckoutBasketResponse>.Failure(
                    new Error(BasketErrorCodes.InvalidCheckoutAddress, BasketErrorCodes.InvalidCheckoutAddress));
            }

            await activeBasketStore.DeleteAsync(customerId, cancellationToken);
            return Result<CheckoutBasketResponse>.Success(
                new CheckoutBasketResponse(
                    existingSnapshot.Id,
                    existingSnapshot.CustomerId,
                    existingSnapshot.TotalAmount,
                    existingSnapshot.Currency,
                    existingSnapshot.CreatedAt));
        }

        BasketEntity? basket = await activeBasketStore.GetAsync(customerId, cancellationToken);

        if (basket is null)
        {
            return Result<CheckoutBasketResponse>.Failure(new Error(ErrorCodes.BasketNotFound, ErrorCodes.BasketNotFound));
        }

        if (basket.Items.Count == 0)
        {
            return Result<CheckoutBasketResponse>.Failure(new Error(BasketErrorCodes.EmptyBasket, BasketErrorCodes.EmptyBasket));
        }

        BasketCheckoutSnapshot snapshot = new(
            request.CheckoutId,
            basket.CustomerId,
            basket.Currency,
            basket.TotalAmount,
            request.RecipientName.Trim(),
            request.AddressLine.Trim(),
            request.City.Trim(),
            request.CountryCode.Trim().ToUpperInvariant(),
            request.PostalCode.Trim());

        foreach (BasketItem item in basket.Items)
        {
            snapshot.AddItem(
                item.ProductId,
                item.ProductName,
                item.Quantity,
                item.UnitPrice,
                item.Currency,
                item.StoreId);
        }

        basketHistoryRepository.Add(snapshot);
        await checkoutPublisher.PublishAsync(snapshot, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await activeBasketStore.DeleteAsync(customerId, cancellationToken);

        return Result<CheckoutBasketResponse>.Success(new CheckoutBasketResponse(snapshot.Id, snapshot.CustomerId, snapshot.TotalAmount, snapshot.Currency, snapshot.CreatedAt));
    }
}
