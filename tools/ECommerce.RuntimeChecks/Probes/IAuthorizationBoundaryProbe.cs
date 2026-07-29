namespace ECommerce.RuntimeChecks.Probes;

internal interface IAuthorizationBoundaryProbe
{
    Task AssertSellerStoreAccessDeniedAsync(CancellationToken cancellationToken);

    Task AssertProductImageUploadDeniedAsync(
        Guid productId,
        CancellationToken cancellationToken);
}
