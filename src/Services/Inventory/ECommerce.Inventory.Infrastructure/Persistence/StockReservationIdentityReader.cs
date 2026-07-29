using ECommerce.Inventory.Application.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Inventory.Infrastructure.Persistence;

public sealed class StockReservationIdentityReader : IStockReservationIdentityReader
{
    private readonly InventoryDbContext dbContext;

    public StockReservationIdentityReader(InventoryDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Guid>> FindIdsByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        return await dbContext.StockReservations
            .AsNoTracking()
            .Where(x => x.OrderId == orderId)
            .Select(x => x.Id)
            .ToArrayAsync(cancellationToken);
    }
}
