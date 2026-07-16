using ECommerce.Inventory.Application.Inventory;
using ECommerce.Inventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Inventory.Infrastructure.Persistence;

public sealed class InventoryRepository : IInventoryRepository
{
    private readonly InventoryDbContext dbContext;

    public InventoryRepository(InventoryDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<InventoryItem?> GetItemAsync(Guid productId, CancellationToken cancellationToken)
    {
        return dbContext.InventoryItems.FirstOrDefaultAsync(x => x.ProductId == productId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<StockReservation>> GetReservationsAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return await dbContext.StockReservations
            .Where(x => x.OrderId == orderId)
            .ToArrayAsync(cancellationToken);
    }

    public void AddItem(InventoryItem item)
    {
        dbContext.InventoryItems.Add(item);
    }

    public void AddReservation(StockReservation reservation)
    {
        dbContext.StockReservations.Add(reservation);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
