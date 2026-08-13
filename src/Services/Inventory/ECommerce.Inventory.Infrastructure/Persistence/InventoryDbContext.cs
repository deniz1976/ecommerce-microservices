using ECommerce.Inventory.Domain;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Inventory.Infrastructure.Persistence;

public sealed class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    public DbSet<StockReservation> StockReservations => Set<StockReservation>();

    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        CapturePendingStockMovements();
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }

    private void CapturePendingStockMovements()
    {
        StockMovement[] movements = ChangeTracker
            .Entries<InventoryItem>()
            .SelectMany(entry => entry.Entity.DequeuePendingMovements())
            .ToArray();

        if (movements.Length > 0)
        {
            StockMovements.AddRange(movements);
        }
    }
}
