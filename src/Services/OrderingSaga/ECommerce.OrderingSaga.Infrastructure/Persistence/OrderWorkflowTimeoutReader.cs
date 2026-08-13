using ECommerce.OrderingSaga.Application.Workflows;
using ECommerce.OrderingSaga.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.OrderingSaga.Infrastructure.Persistence;

public sealed class OrderWorkflowTimeoutReader : IOrderWorkflowTimeoutReader
{
    private readonly OrderingSagaDbContext dbContext;

    public OrderWorkflowTimeoutReader(OrderingSagaDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Guid>> FindDueWorkflowIdsAsync(
        DateTimeOffset now,
        int batchSize,
        CancellationToken cancellationToken)
    {
        return await dbContext.OrderWorkflows
            .AsNoTracking()
            .Where(workflow =>
                workflow.StepDeadlineAt != null &&
                workflow.StepDeadlineAt <= now &&
                workflow.TimeoutHandledAt == null &&
                (workflow.Status == OrderWorkflowStatus.Submitted ||
                    workflow.Status == OrderWorkflowStatus.InventoryReserved ||
                    workflow.Status == OrderWorkflowStatus.PaymentAuthorized))
            .OrderBy(workflow => workflow.StepDeadlineAt)
            .Select(workflow => workflow.Id)
            .Take(batchSize)
            .ToArrayAsync(cancellationToken);
    }
}
