namespace ECommerce.Inventory.Application.Inventory;

public interface IStockReservationIdentityReader
{
    Task<IReadOnlyCollection<Guid>> FindIdsByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken);
}
