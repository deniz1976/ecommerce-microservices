using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.Inventory.Application.Inventory;
using ECommerce.Inventory.Domain;

namespace ECommerce.ContractTests;

internal sealed class FakeInventoryPersistence :
    IRepository<InventoryItem, Guid>,
    IRepository<StockReservation, Guid>,
    IStockReservationIdentityReader,
    IUnitOfWork
{
    private readonly Dictionary<Guid, InventoryItem> items;

    public FakeInventoryPersistence(params InventoryItem[] items)
    {
        this.items = items.ToDictionary(item => item.ProductId);
    }

    public List<StockReservation> Reservations { get; } = [];

    public int SaveChangesCount { get; private set; }

    Task<InventoryItem?> IRepository<InventoryItem, Guid>.GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        items.TryGetValue(id, out InventoryItem? item);
        return Task.FromResult(item);
    }

    void IRepository<InventoryItem, Guid>.Add(InventoryItem entity)
    {
        items.Add(entity.ProductId, entity);
    }

    Task<StockReservation?> IRepository<StockReservation, Guid>.GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Reservations.SingleOrDefault(item => item.Id == id));
    }

    Task<IReadOnlyCollection<Guid>> IStockReservationIdentityReader.FindIdsByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyCollection<Guid>>(
            Reservations.Where(item => item.OrderId == orderId).Select(item => item.Id).ToArray());
    }

    void IRepository<StockReservation, Guid>.Add(StockReservation entity)
    {
        Reservations.Add(entity);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCount++;
        return Task.CompletedTask;
    }
}
