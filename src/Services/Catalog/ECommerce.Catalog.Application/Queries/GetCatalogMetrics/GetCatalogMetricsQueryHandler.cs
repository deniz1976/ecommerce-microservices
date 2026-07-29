using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Catalog.Application.Metrics;

namespace ECommerce.Catalog.Application.Queries.GetCatalogMetrics;

public sealed class GetCatalogMetricsQueryHandler
    : IQueryHandler<GetCatalogMetricsQuery, CatalogMetricsResponse>
{
    private readonly CatalogMetricsService metricsService;

    public GetCatalogMetricsQueryHandler(CatalogMetricsService metricsService)
    {
        this.metricsService = metricsService;
    }

    public Task<CatalogMetricsResponse> HandleAsync(
        GetCatalogMetricsQuery query,
        CancellationToken cancellationToken)
    {
        return metricsService.GetAsync(cancellationToken);
    }
}
