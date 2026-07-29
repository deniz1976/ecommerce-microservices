using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Catalog.Application.Metrics;
using ECommerce.Catalog.Application.Queries.GetCatalogMetrics;

namespace ECommerce.Catalog.Api.Metrics;

public static class CatalogMetricsEndpoints
{
    public static IEndpointRouteBuilder MapCatalogMetricsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/v1/catalog-metrics")
            .RequireAuthorization(AuthorizationPolicies.Admin)
            .WithTags("Catalog Metrics");

        group.MapGet("/", GetAsync)
            .WithName("GetCatalogMetrics");

        return endpoints;
    }

    private static async Task<IResult> GetAsync(
        IQueryHandler<GetCatalogMetricsQuery, CatalogMetricsResponse> queryHandler,
        CancellationToken cancellationToken)
    {
        CatalogMetricsResponse response = await queryHandler.HandleAsync(
            new GetCatalogMetricsQuery(),
            cancellationToken);
        return Results.Ok(response);
    }
}
