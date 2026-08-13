using ECommerce.Basket.Domain;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using BasketEntity = ECommerce.Basket.Domain.Basket;

namespace ECommerce.Basket.Application.Baskets;

public sealed class BasketItemAdditionService(
    IActiveBasketStore activeBasketStore,
    IProductCatalogReader productCatalogReader)
{
    public async Task<Result<BasketResponse>> AddAsync(
        Guid customerId,
        AddBasketItemRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ProductId == Guid.Empty || request.Quantity <= 0)
        {
            return Failure(BasketErrorCodes.InvalidBasketItem);
        }

        Result<CatalogProductSnapshot> productResult =
            await productCatalogReader.GetActiveProductAsync(request.ProductId, cancellationToken);
        if (productResult.IsFailure)
        {
            return Result<BasketResponse>.Failure(productResult.Error!);
        }

        CatalogProductSnapshot product = productResult.Value!;
        BasketEntity basket = await activeBasketStore.GetAsync(customerId, cancellationToken) ??
            new BasketEntity(customerId, product.Currency);
        BasketItemMutationResult mutationResult = basket.AddOrUpdateItem(
            product.Id,
            product.Name,
            request.Quantity,
            product.Price,
            product.Currency,
            product.StoreId);
        if (mutationResult != BasketItemMutationResult.Applied)
        {
            return Failure(mutationResult == BasketItemMutationResult.CurrencyMismatch
                ? BasketErrorCodes.CurrencyMismatch
                : BasketErrorCodes.InvalidBasketItem);
        }

        await activeBasketStore.SaveAsync(basket, cancellationToken);
        return Result<BasketResponse>.Success(basket.ToResponse());
    }

    private static Result<BasketResponse> Failure(string code) =>
        Result<BasketResponse>.Failure(new Error(code, code));
}
