using ECommerce.OrderingSaga.Application.Workflows;
using ECommerce.OrderingSaga.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.OrderingSaga.Infrastructure.Persistence;

public sealed class OrderWorkflowRepository : IOrderWorkflowRepository
{
    private readonly OrderingSagaDbContext dbContext;

    public OrderWorkflowRepository(OrderingSagaDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<OrderWorkflow?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return dbContext.OrderWorkflows
            .Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.OrderId == orderId, cancellationToken);
    }

    public void Add(OrderWorkflow workflow)
    {
        dbContext.OrderWorkflows.Add(workflow);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
