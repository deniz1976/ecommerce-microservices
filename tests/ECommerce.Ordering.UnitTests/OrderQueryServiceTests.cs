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
        OrderQueryService service = new(reader, new FakeStoreOrderAccessAuthorizer());
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

    [Fact]
    public async Task SearchSellerReturnsOnlyAfterOwnershipIsGranted()
    {
        Guid storeId = Guid.NewGuid();
        SellerOrderSummaryResponse item = new(
            Guid.NewGuid(), storeId, OrderStatus.Confirmed, "TRY", 150m,
            2, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        PagedResult<SellerOrderSummaryResponse> page = new([item], 1, 20, 1);
        FakeOrderReader reader = new(
            new PagedResult<OrderSummaryResponse>([], 1, 20, 0), page);
        FakeStoreOrderAccessAuthorizer authorizer = new(StoreOrderAccessResult.Granted);
        OrderQueryService service = new(reader, authorizer);
        SellerOrderListCriteria criteria = new(storeId, 1, 20, null, true);

        Result<PagedResult<SellerOrderSummaryResponse>> result = await service.SearchSellerAsync(
            criteria,
            new SellerOrderAccessContext(false, "seller-token"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(page, result.Value);
        Assert.Equal(criteria, reader.ReceivedSellerCriteria);
        Assert.Equal(storeId, authorizer.ReceivedStoreId);
        Assert.Equal("seller-token", authorizer.ReceivedAccessToken);
    }

    [Fact]
    public async Task SearchSellerDeniesUnknownStoreWithoutQueryingOrders()
    {
        FakeOrderReader reader = new(new PagedResult<OrderSummaryResponse>([], 1, 20, 0));
        OrderQueryService service = new(
            reader,
            new FakeStoreOrderAccessAuthorizer(StoreOrderAccessResult.Denied));

        Result<PagedResult<SellerOrderSummaryResponse>> result = await service.SearchSellerAsync(
            new SellerOrderListCriteria(Guid.NewGuid(), 1, 20, null, true),
            new SellerOrderAccessContext(false, "seller-token"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("ACCESS_DENIED", result.Error!.Code);
        Assert.Null(reader.ReceivedSellerCriteria);
    }

    [Fact]
    public async Task SearchSellerMapsCatalogFailureWithoutQueryingOrders()
    {
        FakeOrderReader reader = new(new PagedResult<OrderSummaryResponse>([], 1, 20, 0));
        OrderQueryService service = new(
            reader,
            new FakeStoreOrderAccessAuthorizer(StoreOrderAccessResult.DependencyUnavailable));

        Result<PagedResult<SellerOrderSummaryResponse>> result = await service.SearchSellerAsync(
            new SellerOrderListCriteria(Guid.NewGuid(), 1, 20, null, true),
            new SellerOrderAccessContext(false, "seller-token"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("DEPENDENCY_UNAVAILABLE", result.Error!.Code);
        Assert.Null(reader.ReceivedSellerCriteria);
    }

    [Fact]
    public async Task GetSellerReturnsStoreScopedDetailAfterOwnershipIsGranted()
    {
        Guid storeId = Guid.NewGuid();
        Guid orderId = Guid.NewGuid();
        SellerOrderDetailResponse detail = new(
            orderId,
            storeId,
            OrderStatus.Confirmed,
            "TRY",
            150m,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            []);
        FakeOrderReader reader = new(
            new PagedResult<OrderSummaryResponse>([], 1, 20, 0),
            sellerDetailResult: detail);
        OrderQueryService service = new(reader, new FakeStoreOrderAccessAuthorizer());

        Result<SellerOrderDetailResponse> result = await service.GetSellerAsync(
            storeId,
            orderId,
            new SellerOrderAccessContext(false, "seller-token"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(detail, result.Value);
        Assert.Equal(storeId, reader.ReceivedSellerStoreId);
        Assert.Equal(orderId, reader.ReceivedSellerOrderId);
    }

    [Fact]
    public async Task GetSellerConcealsAnOrderOutsideTheSelectedStore()
    {
        FakeOrderReader reader = new(new PagedResult<OrderSummaryResponse>([], 1, 20, 0));
        OrderQueryService service = new(reader, new FakeStoreOrderAccessAuthorizer());

        Result<SellerOrderDetailResponse> result = await service.GetSellerAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new SellerOrderAccessContext(false, "seller-token"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("ORDER_NOT_FOUND", result.Error!.Code);
    }

    [Fact]
    public async Task GetSellerDeniesUnknownStoreWithoutQueryingOrderDetail()
    {
        FakeOrderReader reader = new(new PagedResult<OrderSummaryResponse>([], 1, 20, 0));
        OrderQueryService service = new(
            reader,
            new FakeStoreOrderAccessAuthorizer(StoreOrderAccessResult.Denied));

        Result<SellerOrderDetailResponse> result = await service.GetSellerAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new SellerOrderAccessContext(false, "seller-token"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("ACCESS_DENIED", result.Error!.Code);
        Assert.Null(reader.ReceivedSellerOrderId);
    }
}
