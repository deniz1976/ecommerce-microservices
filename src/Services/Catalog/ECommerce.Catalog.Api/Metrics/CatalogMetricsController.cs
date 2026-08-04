using ECommerce.BuildingBlocks.Security;
using ECommerce.Catalog.Application.Metrics;
using ECommerce.Catalog.Application.Queries.GetCatalogMetrics;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Catalog.Api.Metrics;

[ApiController]
[Route("api/v1/catalog-metrics")]
[Authorize(Policy = AuthorizationPolicies.Admin)]
public sealed class CatalogMetricsController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "GetCatalogMetrics")]
    public async Task<CatalogMetricsResponse> GetAsync(CancellationToken cancellationToken)
    {
        return await sender.Send(new GetCatalogMetricsQuery(), cancellationToken);
    }
}
