using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.OrderingSaga.Application.Workflows;
using ECommerce.OrderingSaga.Domain;

namespace ECommerce.Ordering.UnitTests;

internal sealed class OrderWorkflowFakeRepository :
    IRepository<OrderWorkflow, Guid>,
    IUnitOfWork,
    IOrderWorkflowIdentityReader
{
    public List<OrderWorkflow> Workflows { get; } = [];

    public int SaveCount { get; private set; }

    public Task<OrderWorkflow?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Workflows.FirstOrDefault(workflow => workflow.Id == id));
    }

    public void Add(OrderWorkflow entity)
    {
        Workflows.Add(entity);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.CompletedTask;
    }

    public Task<Guid?> FindIdByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(
            Workflows
                .Where(workflow => workflow.OrderId == orderId)
                .Select(workflow => (Guid?)workflow.Id)
                .SingleOrDefault());
    }
}
