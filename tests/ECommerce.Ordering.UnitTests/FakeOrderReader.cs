using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Orders;

namespace ECommerce.Ordering.UnitTests;

internal sealed class FakeOrderReader : IOrderReader
{
    private readonly PagedResult<OrderSummaryResponse> result;

    public FakeOrderReader(PagedResult<OrderSummaryResponse> result)
    {
        this.result = result;
    }

    public OrderListCriteria? ReceivedCriteria { get; private set; }

    public Task<PagedResult<OrderSummaryResponse>> SearchAsync(
        OrderListCriteria criteria,
        CancellationToken cancellationToken)
    {
        ReceivedCriteria = criteria;
        return Task.FromResult(result);
    }
}
