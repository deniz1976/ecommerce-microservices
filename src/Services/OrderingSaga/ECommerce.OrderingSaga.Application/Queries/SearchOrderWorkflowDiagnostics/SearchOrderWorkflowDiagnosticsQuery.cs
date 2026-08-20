using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.OrderingSaga.Application.Diagnostics;

namespace ECommerce.OrderingSaga.Application.Queries.SearchOrderWorkflowDiagnostics;

public sealed record SearchOrderWorkflowDiagnosticsQuery(OrderWorkflowDiagnosticsCriteria Criteria)
    : IQuery<PagedResult<OrderWorkflowDiagnosticsResponse>>;
