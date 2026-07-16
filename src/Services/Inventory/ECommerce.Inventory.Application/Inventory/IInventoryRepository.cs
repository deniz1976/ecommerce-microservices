using ECommerce.Inventory.Domain;

namespace ECommerce.Inventory.Application.Inventory;

public interface IInventoryRepository
{
    Task<InventoryItem?> GetItemAsync(Guid productId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<StockReservation>> GetReservationsAsync(Guid orderId, CancellationToken cancellationToken);

    void AddItem(InventoryItem item);

    void AddReservation(StockReservation reservation);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
