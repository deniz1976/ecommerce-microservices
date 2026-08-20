using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Security;
using ECommerce.OrderingSaga.Application.Diagnostics;
using ECommerce.OrderingSaga.Application.Queries.SearchOrderWorkflowDiagnostics;
using ECommerce.OrderingSaga.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.OrderingSaga.Worker.Diagnostics;

[ApiController]
[Route("api/v1/workflows/diagnostics")]
[Authorize(Policy = AuthorizationPolicies.Admin)]
public sealed class OrderWorkflowDiagnosticsController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "SearchOrderWorkflowDiagnostics")]
    public Task<PagedResult<OrderWorkflowDiagnosticsResponse>> SearchAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? orderId = null,
        [FromQuery] OrderWorkflowStatus? status = null,
        [FromQuery] bool overdueOnly = false,
        CancellationToken cancellationToken = default)
    {
        return sender.Send(
            new SearchOrderWorkflowDiagnosticsQuery(
                new OrderWorkflowDiagnosticsCriteria(
                    pageNumber,
                    pageSize,
                    orderId,
                    status,
                    overdueOnly)),
            cancellationToken);
    }
}
