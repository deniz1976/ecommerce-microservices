using ECommerce.OrderingSaga.Domain;

namespace ECommerce.OrderingSaga.Application.Workflows;

public interface IOrderWorkflowRepository
{
    Task<OrderWorkflow?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);

    void Add(OrderWorkflow workflow);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
