using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.Application.Orders;

public sealed class OrderStatusService
{
    private readonly IRepository<Order, Guid> repository;
    private readonly IUnitOfWork unitOfWork;

    public OrderStatusService(IRepository<Order, Guid> repository, IUnitOfWork unitOfWork)
    {
        this.repository = repository;
        this.unitOfWork = unitOfWork;
    }

    public async Task ConfirmAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken)
    {
        await UpdateAsync(orderId, customerId, order => order.MarkConfirmed(), cancellationToken);
    }

    public async Task CancelAsync(
        Guid orderId,
        Guid customerId,
        string? reasonCode,
        CancellationToken cancellationToken)
    {
        string safeReasonCode = CustomerSafeCancellationReason.Normalize(reasonCode);
        await UpdateAsync(orderId, customerId, order => order.MarkCancelled(safeReasonCode), cancellationToken);
    }

    public Task InventoryReservedAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken)
    {
        return UpdateAsync(orderId, customerId, order => order.MarkInventoryReserved(), cancellationToken);
    }

    public Task PaymentAuthorizedAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken)
    {
        return UpdateAsync(orderId, customerId, order => order.MarkPaymentAuthorized(), cancellationToken);
    }

    public Task ShipmentCreatedAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken)
    {
        return UpdateAsync(orderId, customerId, order => order.MarkShipmentCreated(), cancellationToken);
    }

    private async Task UpdateAsync(
        Guid orderId,
        Guid customerId,
        Action<Order> update,
        CancellationToken cancellationToken)
    {
        Order? order = await repository.GetByIdAsync(orderId, cancellationToken);
        if (order is null || order.CustomerId != customerId)
        {
            return;
        }

        OrderStatus statusBeforeUpdate = order.Status;
        update(order);
        if (order.Status != statusBeforeUpdate)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
