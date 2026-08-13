using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.OrderingSaga.Domain;

namespace ECommerce.OrderingSaga.Application.Workflows;

public sealed class OrderWorkflowLoader(
    IRepository<OrderWorkflow, Guid> repository,
    IOrderWorkflowIdentityReader identityReader)
{
    public async Task<OrderWorkflow?> LoadByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        Guid? workflowId = await identityReader.FindIdByOrderIdAsync(orderId, cancellationToken);
        return workflowId is null
            ? null
            : await repository.GetByIdAsync(workflowId.Value, cancellationToken);
    }
}
