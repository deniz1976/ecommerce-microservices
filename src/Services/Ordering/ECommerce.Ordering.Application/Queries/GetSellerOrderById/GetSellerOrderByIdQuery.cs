using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Orders;

namespace ECommerce.Ordering.Application.Queries.GetSellerOrderById;

public sealed record GetSellerOrderByIdQuery(
    Guid StoreId,
    Guid OrderId,
    SellerOrderAccessContext AccessContext)
    : IQuery<Result<SellerOrderDetailResponse>>;
