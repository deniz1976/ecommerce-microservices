using ECommerce.Basket.Application.Baskets;
using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Basket.Application.Queries.GetBasket;

public sealed class GetBasketQueryHandler
    : IQueryHandler<GetBasketQuery, Result<BasketResponse>>
{
    private readonly BasketQueryService basketQueryService;

    public GetBasketQueryHandler(BasketQueryService basketQueryService)
    {
        this.basketQueryService = basketQueryService;
    }

    public Task<Result<BasketResponse>> HandleAsync(
        GetBasketQuery query,
        CancellationToken cancellationToken)
    {
        return basketQueryService.GetAsync(query.CustomerId, cancellationToken);
    }
}
