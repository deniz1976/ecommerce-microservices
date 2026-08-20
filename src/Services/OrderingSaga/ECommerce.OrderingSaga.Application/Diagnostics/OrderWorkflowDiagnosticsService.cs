using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.OrderingSaga.Application.Diagnostics;

public sealed class OrderWorkflowDiagnosticsService(IOrderWorkflowDiagnosticsReader reader)
{
    public Task<PagedResult<OrderWorkflowDiagnosticsResponse>> SearchAsync(
        OrderWorkflowDiagnosticsCriteria criteria,
        CancellationToken cancellationToken)
    {
        return reader.SearchAsync(criteria, cancellationToken);
    }
}
