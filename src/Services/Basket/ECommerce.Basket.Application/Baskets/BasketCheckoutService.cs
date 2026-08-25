using ECommerce.Basket.Domain;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using BasketEntity = ECommerce.Basket.Domain.Basket;

namespace ECommerce.Basket.Application.Baskets;

public sealed class BasketCheckoutService
{
    private const int MaximumCheckoutItemCount = 100;
    private readonly IActiveBasketStore activeBasketStore;
    private readonly IRepository<BasketCheckoutSnapshot, Guid> basketHistoryRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly ICheckoutPublisher checkoutPublisher;

    public BasketCheckoutService(
        IActiveBasketStore activeBasketStore,
        IRepository<BasketCheckoutSnapshot, Guid> basketHistoryRepository,
        IUnitOfWork unitOfWork,
        ICheckoutPublisher checkoutPublisher)
    {
        this.activeBasketStore = activeBasketStore;
        this.basketHistoryRepository = basketHistoryRepository;
        this.unitOfWork = unitOfWork;
        this.checkoutPublisher = checkoutPublisher;
    }

    public async Task<Result<CheckoutBasketResponse>> CheckoutAsync(
        Guid customerId,
        CheckoutBasketRequest request,
        CancellationToken cancellationToken)
    {
        if (request.CheckoutId == Guid.Empty ||
            string.IsNullOrWhiteSpace(request.RecipientName) ||
            request.RecipientName.Trim().Length > 256 ||
            string.IsNullOrWhiteSpace(request.AddressLine) ||
            request.AddressLine.Trim().Length > 512 ||
            string.IsNullOrWhiteSpace(request.City) ||
            request.City.Trim().Length > 128 ||
            request.CountryCode.Trim().Length != 2 ||
            string.IsNullOrWhiteSpace(request.PostalCode) ||
            request.PostalCode.Trim().Length > 32)
        {
            return Failure(BasketErrorCodes.InvalidCheckoutAddress);
        }

        BasketCheckoutSnapshot? existingSnapshot =
            await basketHistoryRepository.GetByIdAsync(
                request.CheckoutId,
                cancellationToken);
        if (existingSnapshot is not null)
        {
            if (existingSnapshot.CustomerId != customerId)
            {
                return Failure(BasketErrorCodes.InvalidCheckoutAddress);
            }

            await activeBasketStore.DeleteAsync(customerId, cancellationToken);
            return Result<CheckoutBasketResponse>.Success(ToResponse(existingSnapshot));
        }

        BasketEntity? basket = await activeBasketStore.GetAsync(customerId, cancellationToken);
        if (basket is null)
        {
            return Failure(ErrorCodes.BasketNotFound);
        }

        if (basket.Items.Count == 0)
        {
            return Failure(BasketErrorCodes.EmptyBasket);
        }

        if (basket.Items.Count > MaximumCheckoutItemCount)
        {
            return Failure(BasketErrorCodes.InvalidCheckoutAddress);
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

        return Result<CheckoutBasketResponse>.Success(ToResponse(snapshot));
    }

    private static CheckoutBasketResponse ToResponse(BasketCheckoutSnapshot snapshot)
    {
        return new CheckoutBasketResponse(
            snapshot.Id,
            snapshot.CustomerId,
            snapshot.TotalAmount,
            snapshot.Currency,
            snapshot.CreatedAt);
    }

    private static Result<CheckoutBasketResponse> Failure(string code)
    {
        return Result<CheckoutBasketResponse>.Failure(new Error(code, code));
    }
}
