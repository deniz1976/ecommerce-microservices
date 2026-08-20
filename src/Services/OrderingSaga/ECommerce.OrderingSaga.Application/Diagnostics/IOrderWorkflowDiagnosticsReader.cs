using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.OrderingSaga.Application.Diagnostics;

public interface IOrderWorkflowDiagnosticsReader
{
    Task<PagedResult<OrderWorkflowDiagnosticsResponse>> SearchAsync(
        OrderWorkflowDiagnosticsCriteria criteria,
        CancellationToken cancellationToken);
}
