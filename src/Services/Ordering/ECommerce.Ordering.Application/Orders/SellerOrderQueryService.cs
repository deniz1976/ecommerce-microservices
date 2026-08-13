using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Ordering.Application.Orders;

public sealed class SellerOrderQueryService(
    IOrderReader orderReader,
    IStoreOrderAccessAuthorizer storeAccessAuthorizer)
{
    public async Task<Result<PagedResult<SellerOrderSummaryResponse>>> SearchAsync(
        SellerOrderListCriteria criteria,
        SellerOrderAccessContext accessContext,
        CancellationToken cancellationToken)
    {
        Error? authorizationError = await GetAuthorizationErrorAsync(
            criteria.StoreId,
            accessContext,
            cancellationToken);
        if (authorizationError is not null)
        {
            return Result<PagedResult<SellerOrderSummaryResponse>>.Failure(authorizationError);
        }

        PagedResult<SellerOrderSummaryResponse> page = await orderReader.SearchSellerAsync(
            criteria,
            cancellationToken);
        return Result<PagedResult<SellerOrderSummaryResponse>>.Success(page);
    }

    public async Task<Result<SellerOrderDetailResponse>> GetByIdAsync(
        Guid storeId,
        Guid orderId,
        SellerOrderAccessContext accessContext,
        CancellationToken cancellationToken)
    {
        Error? authorizationError = await GetAuthorizationErrorAsync(
            storeId,
            accessContext,
            cancellationToken);
        if (authorizationError is not null)
        {
            return Result<SellerOrderDetailResponse>.Failure(authorizationError);
        }

        SellerOrderDetailResponse? order = await orderReader.GetSellerAsync(
            storeId,
            orderId,
            cancellationToken);
        return order is null
            ? Result<SellerOrderDetailResponse>.Failure(
                new Error(ErrorCodes.OrderNotFound, ErrorCodes.OrderNotFound))
            : Result<SellerOrderDetailResponse>.Success(order);
    }

    private async Task<Error?> GetAuthorizationErrorAsync(
        Guid storeId,
        SellerOrderAccessContext accessContext,
        CancellationToken cancellationToken)
    {
        if (accessContext.BypassStoreOwnership)
        {
            return null;
        }

        StoreOrderAccessResult accessResult = await storeAccessAuthorizer.AuthorizeAsync(
            storeId,
            accessContext.AccessToken,
            cancellationToken);
        return accessResult switch
        {
            StoreOrderAccessResult.Granted => null,
            StoreOrderAccessResult.DependencyUnavailable => new Error(
                ErrorCodes.DependencyUnavailable,
                ErrorCodes.DependencyUnavailable),
            _ => new Error(ErrorCodes.AccessDenied, ErrorCodes.AccessDenied)
        };
    }
}
