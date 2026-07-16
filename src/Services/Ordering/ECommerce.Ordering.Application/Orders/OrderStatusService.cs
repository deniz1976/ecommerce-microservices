using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.Application.Orders;

public sealed class OrderStatusService
{
    private readonly IOrderRepository repository;

    public OrderStatusService(IOrderRepository repository)
    {
        this.repository = repository;
    }

    public async Task ConfirmAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken)
    {
        Order? order = await repository.GetByIdAsync(orderId, cancellationToken);

        if (order is null || order.CustomerId != customerId || order.Status != OrderStatus.Submitted)
        {
            return;
        }

        order.MarkConfirmed();
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken)
    {
        Order? order = await repository.GetByIdAsync(orderId, cancellationToken);

        if (order is null || order.CustomerId != customerId || order.Status != OrderStatus.Submitted)
        {
            return;
        }

        order.MarkCancelled();
        await repository.SaveChangesAsync(cancellationToken);
    }
}
