using ECommerce.Basket.Application.Baskets;
using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Basket.Application.Commands.CheckoutBasket;

public sealed record CheckoutBasketCommand(Guid CustomerId, CheckoutBasketRequest Request)
    : ICommand<Result<CheckoutBasketResponse>>;
