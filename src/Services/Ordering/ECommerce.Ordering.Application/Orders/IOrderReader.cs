using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Ordering.Application.Orders;

public interface IOrderReader
{
    Task<PagedResult<OrderSummaryResponse>> SearchAsync(
        OrderListCriteria criteria,
        CancellationToken cancellationToken);

    Task<PagedResult<SellerOrderSummaryResponse>> SearchSellerAsync(
        SellerOrderListCriteria criteria,
        CancellationToken cancellationToken);

    Task<SellerOrderDetailResponse?> GetSellerAsync(
        Guid storeId,
        Guid orderId,
        CancellationToken cancellationToken);
}
