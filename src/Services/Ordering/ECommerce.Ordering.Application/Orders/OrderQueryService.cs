using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Ordering.Application.Orders;

public sealed class OrderQueryService
{
    private readonly IOrderReader orderReader;
    private readonly IStoreOrderAccessAuthorizer storeAccessAuthorizer;

    public OrderQueryService(
        IOrderReader orderReader,
        IStoreOrderAccessAuthorizer storeAccessAuthorizer)
    {
        this.orderReader = orderReader;
        this.storeAccessAuthorizer = storeAccessAuthorizer;
    }

    public async Task<Result<PagedResult<OrderSummaryResponse>>> SearchAsync(
        OrderListCriteria criteria,
        CancellationToken cancellationToken)
    {
        PagedResult<OrderSummaryResponse> page = await orderReader.SearchAsync(
            criteria,
            cancellationToken);
        return Result<PagedResult<OrderSummaryResponse>>.Success(page);
    }

    public async Task<Result<PagedResult<SellerOrderSummaryResponse>>> SearchSellerAsync(
        SellerOrderListCriteria criteria,
        SellerOrderAccessContext accessContext,
        CancellationToken cancellationToken)
    {
        if (!accessContext.BypassStoreOwnership)
        {
            StoreOrderAccessResult accessResult = await storeAccessAuthorizer.AuthorizeAsync(
                criteria.StoreId,
                accessContext.AccessToken,
                cancellationToken);
            if (accessResult == StoreOrderAccessResult.DependencyUnavailable)
            {
                return Result<PagedResult<SellerOrderSummaryResponse>>.Failure(
                    new Error(ErrorCodes.DependencyUnavailable, ErrorCodes.DependencyUnavailable));
            }

            if (accessResult != StoreOrderAccessResult.Granted)
            {
                return Result<PagedResult<SellerOrderSummaryResponse>>.Failure(
                    new Error(ErrorCodes.AccessDenied, ErrorCodes.AccessDenied));
            }
        }

        PagedResult<SellerOrderSummaryResponse> page = await orderReader.SearchSellerAsync(
            criteria,
            cancellationToken);
        return Result<PagedResult<SellerOrderSummaryResponse>>.Success(page);
    }
}
