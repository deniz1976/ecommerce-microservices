using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Basket.Application.Commands.ClearBasket;

public sealed record ClearBasketCommand(Guid CustomerId) : ICommand<Result>;
