using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.OrderingSaga.Application.Diagnostics;
using ECommerce.OrderingSaga.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.OrderingSaga.Infrastructure.Persistence;

public sealed class OrderWorkflowDiagnosticsReader(
    OrderingSagaDbContext dbContext,
    TimeProvider timeProvider)
    : IOrderWorkflowDiagnosticsReader
{
    private const int MaximumPageSize = 50;

    public async Task<PagedResult<OrderWorkflowDiagnosticsResponse>> SearchAsync(
        OrderWorkflowDiagnosticsCriteria criteria,
        CancellationToken cancellationToken)
    {
        int pageNumber = Math.Max(1, criteria.PageNumber);
        int pageSize = Math.Clamp(criteria.PageSize, 1, MaximumPageSize);
        DateTimeOffset now = timeProvider.GetUtcNow();
        IQueryable<OrderWorkflow> workflows = dbContext.OrderWorkflows.AsNoTracking();

        if (criteria.OrderId.HasValue)
        {
            workflows = workflows.Where(workflow => workflow.OrderId == criteria.OrderId.Value);
        }

        if (criteria.Status.HasValue)
        {
            workflows = workflows.Where(workflow => workflow.Status == criteria.Status.Value);
        }

        if (criteria.OverdueOnly)
        {
            workflows = workflows.Where(workflow =>
                workflow.StepDeadlineAt.HasValue &&
                workflow.StepDeadlineAt.Value <= now &&
                !workflow.TimeoutHandledAt.HasValue &&
                (workflow.Status == OrderWorkflowStatus.Submitted ||
                 workflow.Status == OrderWorkflowStatus.InventoryReserved ||
                 workflow.Status == OrderWorkflowStatus.PaymentAuthorized));
        }

        int totalCount = await workflows.CountAsync(cancellationToken);
        OrderWorkflowDiagnosticsResponse[] items = await workflows
            .OrderByDescending(workflow => workflow.UpdatedAt)
            .ThenBy(workflow => workflow.OrderId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(workflow => new OrderWorkflowDiagnosticsResponse(
                workflow.OrderId,
                workflow.Status,
                workflow.StepDeadlineAt,
                workflow.TimeoutHandledAt,
                workflow.CreatedAt,
                workflow.UpdatedAt,
                workflow.StepDeadlineAt.HasValue &&
                workflow.StepDeadlineAt.Value <= now &&
                !workflow.TimeoutHandledAt.HasValue &&
                (workflow.Status == OrderWorkflowStatus.Submitted ||
                 workflow.Status == OrderWorkflowStatus.InventoryReserved ||
                 workflow.Status == OrderWorkflowStatus.PaymentAuthorized)))
            .ToArrayAsync(cancellationToken);

        return new PagedResult<OrderWorkflowDiagnosticsResponse>(
            items,
            pageNumber,
            pageSize,
            totalCount);
    }
}
