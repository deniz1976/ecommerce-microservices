using ECommerce.OrderingSaga.Application.Workflows;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.OrderingSaga.Infrastructure.Persistence;

public sealed class OrderWorkflowIdentityReader : IOrderWorkflowIdentityReader
{
    private readonly OrderingSagaDbContext dbContext;

    public OrderWorkflowIdentityReader(OrderingSagaDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<Guid?> FindIdByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return dbContext.OrderWorkflows
            .AsNoTracking()
            .Where(workflow => workflow.OrderId == orderId)
            .Select(workflow => (Guid?)workflow.Id)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
