using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Orders;

namespace ECommerce.Ordering.UnitTests;

internal sealed class FakeOrderReader : IOrderReader
{
    private readonly PagedResult<OrderSummaryResponse> result;
    private readonly PagedResult<SellerOrderSummaryResponse> sellerResult;
    private readonly SellerOrderDetailResponse? sellerDetailResult;

    public FakeOrderReader(
        PagedResult<OrderSummaryResponse> result,
        PagedResult<SellerOrderSummaryResponse>? sellerResult = null,
        SellerOrderDetailResponse? sellerDetailResult = null)
    {
        this.result = result;
        this.sellerResult = sellerResult ??
            new PagedResult<SellerOrderSummaryResponse>([], 1, 20, 0);
        this.sellerDetailResult = sellerDetailResult;
    }

    public OrderListCriteria? ReceivedCriteria { get; private set; }

    public SellerOrderListCriteria? ReceivedSellerCriteria { get; private set; }

    public Guid? ReceivedSellerStoreId { get; private set; }

    public Guid? ReceivedSellerOrderId { get; private set; }

    public Task<PagedResult<OrderSummaryResponse>> SearchAsync(
        OrderListCriteria criteria,
        CancellationToken cancellationToken)
    {
        ReceivedCriteria = criteria;
        return Task.FromResult(result);
    }

    public Task<PagedResult<SellerOrderSummaryResponse>> SearchSellerAsync(
        SellerOrderListCriteria criteria,
        CancellationToken cancellationToken)
    {
        ReceivedSellerCriteria = criteria;
        return Task.FromResult(sellerResult);
    }

    public Task<SellerOrderDetailResponse?> GetSellerAsync(
        Guid storeId,
        Guid orderId,
        CancellationToken cancellationToken)
    {
        ReceivedSellerStoreId = storeId;
        ReceivedSellerOrderId = orderId;
        return Task.FromResult(sellerDetailResult);
    }
}
