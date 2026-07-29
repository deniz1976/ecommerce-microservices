using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Orders;

namespace ECommerce.Ordering.Application.Commands.CreateOrderFromCheckout;

public sealed record CreateOrderFromCheckoutCommand(BasketCheckedOut Checkout)
    : ICommand<Result<OrderResponse>>;
