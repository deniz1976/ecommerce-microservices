using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Basket.Application.Baskets;

namespace ECommerce.Basket.Application.Queries.GetBasket;

public sealed record GetBasketQuery(Guid CustomerId) : IQuery<Result<BasketResponse>>;
