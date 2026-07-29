namespace ECommerce.Catalog.Application.Metrics;

public sealed class CatalogMetricsService
{
    private readonly ICatalogMetricsReader metricsReader;

    public CatalogMetricsService(ICatalogMetricsReader metricsReader)
    {
        this.metricsReader = metricsReader;
    }

    public Task<CatalogMetricsResponse> GetAsync(CancellationToken cancellationToken)
    {
        return metricsReader.ReadAsync(cancellationToken);
    }
}
