using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Orders;

namespace ECommerce.Ordering.Application.Queries.GetOrdersByCustomer;

public sealed record GetOrdersByCustomerQuery(Guid CustomerId)
    : IQuery<Result<IReadOnlyCollection<OrderResponse>>>;
