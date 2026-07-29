namespace ECommerce.RuntimeChecks.Probes;

internal interface ICatalogProductFixture
{
    Task CreateAsync(
        Guid productId,
        string productName,
        decimal unitPrice,
        string currency,
        CancellationToken cancellationToken);

    Task DeleteAsync(Guid productId, CancellationToken cancellationToken);
}
