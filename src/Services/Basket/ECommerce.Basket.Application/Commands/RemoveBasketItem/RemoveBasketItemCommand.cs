using ECommerce.Basket.Application.Baskets;
using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Basket.Application.Commands.RemoveBasketItem;

public sealed record RemoveBasketItemCommand(Guid CustomerId, Guid ProductId)
    : ICommand<Result<BasketResponse>>;
