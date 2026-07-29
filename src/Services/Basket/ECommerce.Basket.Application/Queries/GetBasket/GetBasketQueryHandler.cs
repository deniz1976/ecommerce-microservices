using ECommerce.Basket.Application.Baskets;
using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Basket.Application.Queries.GetBasket;

public sealed class GetBasketQueryHandler
    : IQueryHandler<GetBasketQuery, Result<BasketResponse>>
{
    private readonly BasketService basketService;

    public GetBasketQueryHandler(BasketService basketService)
    {
        this.basketService = basketService;
    }

    public Task<Result<BasketResponse>> HandleAsync(
        GetBasketQuery query,
        CancellationToken cancellationToken)
    {
        return basketService.GetAsync(query.CustomerId, cancellationToken);
    }
}
