using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Orders;

namespace ECommerce.Ordering.Application.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid Id) : IQuery<Result<OrderResponse>>;
