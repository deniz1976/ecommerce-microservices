using ECommerce.Basket.Application.Baskets;
using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Basket.Application.Commands.AddBasketItem;

public sealed record AddBasketItemCommand(Guid CustomerId, AddBasketItemRequest Request)
    : ICommand<Result<BasketResponse>>;
