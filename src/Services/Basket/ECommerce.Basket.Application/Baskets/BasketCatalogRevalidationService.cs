using ECommerce.Basket.Domain;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using BasketEntity = ECommerce.Basket.Domain.Basket;

namespace ECommerce.Basket.Application.Baskets;

public sealed class BasketCatalogRevalidationService(
    IActiveBasketStore activeBasketStore,
    IProductCatalogReader productCatalogReader)
{
    public async Task<Result> RevalidateAsync(
        BasketEntity basket,
        CancellationToken cancellationToken)
    {
        Result<CatalogProductSnapshot>[] products = await Task.WhenAll(
            basket.Items.Select(item =>
                productCatalogReader.GetActiveProductAsync(item.ProductId, cancellationToken)));

        bool priceChanged = false;
        foreach (Result<CatalogProductSnapshot> productResult in products)
        {
            if (productResult.IsFailure)
            {
                return productResult.Error!.Code == ErrorCodes.ProductNotFound
                    ? Result.Failure(new Error(
                        BasketErrorCodes.BasketItemUnavailable,
                        BasketErrorCodes.BasketItemUnavailable))
                    : Result.Failure(productResult.Error!);
            }

            CatalogProductSnapshot product = productResult.Value!;
            BasketItemRefreshResult itemResult = basket.RefreshItem(
                product.Id,
                product.Name,
                product.Price,
                product.Currency,
                product.StoreId);
            if (itemResult == BasketItemRefreshResult.Rejected)
            {
                return Result.Failure(new Error(
                    BasketErrorCodes.BasketItemUnavailable,
                    BasketErrorCodes.BasketItemUnavailable));
            }

            priceChanged |= itemResult == BasketItemRefreshResult.PriceChanged;
        }

        if (!priceChanged)
        {
            return Result.Success();
        }

        Result saveResult = await activeBasketStore.SaveAsync(basket, cancellationToken);
        return saveResult.IsFailure
            ? saveResult
            : Result.Failure(new Error(
                BasketErrorCodes.BasketPricesChanged,
                BasketErrorCodes.BasketPricesChanged));
    }
}
