namespace ECommerce.OrderingSaga.Application.Workflows;

public interface IOrderWorkflowIdentityReader
{
    Task<Guid?> FindIdByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);
}
