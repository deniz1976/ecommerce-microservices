using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Orders;
using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.UnitTests;

public sealed class OrderQueryServiceTests
{
    [Fact]
    public async Task SearchForwardsBoundedCriteriaAndReturnsReaderPage()
    {
        OrderSummaryResponse item = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TRY",
            OrderStatus.Confirmed,
            1250m,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow);
        PagedResult<OrderSummaryResponse> page = new([item], 2, 10, 16);
        FakeOrderReader reader = new(page);
        OrderQueryService service = new(reader);
        OrderListCriteria criteria = new(
            item.CustomerId,
            2,
            10,
            OrderStatus.Confirmed,
            SortDescending: true);

        Result<PagedResult<OrderSummaryResponse>> result = await service.SearchAsync(
            criteria,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(page, result.Value);
        Assert.Equal(criteria, reader.ReceivedCriteria);
    }
}
