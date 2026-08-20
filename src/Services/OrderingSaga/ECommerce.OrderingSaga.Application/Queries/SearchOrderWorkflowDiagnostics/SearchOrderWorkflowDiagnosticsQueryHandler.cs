using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.OrderingSaga.Application.Diagnostics;

namespace ECommerce.OrderingSaga.Application.Queries.SearchOrderWorkflowDiagnostics;

public sealed class SearchOrderWorkflowDiagnosticsQueryHandler(
    OrderWorkflowDiagnosticsService service)
    : IQueryHandler<SearchOrderWorkflowDiagnosticsQuery, PagedResult<OrderWorkflowDiagnosticsResponse>>
{
    public Task<PagedResult<OrderWorkflowDiagnosticsResponse>> HandleAsync(
        SearchOrderWorkflowDiagnosticsQuery query,
        CancellationToken cancellationToken)
    {
        return service.SearchAsync(query.Criteria, cancellationToken);
    }
}
