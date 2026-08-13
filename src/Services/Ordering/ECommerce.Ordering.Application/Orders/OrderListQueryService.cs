using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Ordering.Application.Orders;

public sealed class OrderListQueryService(IOrderReader orderReader)
{
    public async Task<Result<PagedResult<OrderSummaryResponse>>> SearchAsync(
        OrderListCriteria criteria,
        CancellationToken cancellationToken)
    {
        PagedResult<OrderSummaryResponse> page = await orderReader.SearchAsync(
            criteria,
            cancellationToken);
        return Result<PagedResult<OrderSummaryResponse>>.Success(page);
    }
}
