using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Catalog.Application.Metrics;

namespace ECommerce.Catalog.Application.Queries.GetCatalogMetrics;

public sealed record GetCatalogMetricsQuery : IQuery<CatalogMetricsResponse>;
