namespace ECommerce.Catalog.Application.Metrics;

public interface ICatalogMetricsReader
{
    Task<CatalogMetricsResponse> ReadAsync(CancellationToken cancellationToken);
}
