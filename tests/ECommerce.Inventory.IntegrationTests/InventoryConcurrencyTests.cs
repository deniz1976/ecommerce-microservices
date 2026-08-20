using ECommerce.Inventory.Domain;
using ECommerce.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Inventory.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class InventoryConcurrencyTests(PostgreSqlFixture fixture)
{
    [Fact]
    public async Task Concurrent_stock_updates_reject_the_stale_writer()
    {
        DbContextOptions<InventoryDbContext> options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseNpgsql(fixture.ConnectionString)
            .Options;

        await using (InventoryDbContext setupContext = new(options))
        {
            await setupContext.Database.MigrateAsync();
            setupContext.InventoryItems.Add(new InventoryItem(Guid.NewGuid(), 10));
            await setupContext.SaveChangesAsync();
        }

        await using InventoryDbContext firstContext = new(options);
        await using InventoryDbContext staleContext = new(options);
        InventoryItem firstWriter = await firstContext.InventoryItems.SingleAsync();
        InventoryItem staleWriter = await staleContext.InventoryItems.SingleAsync();

        firstWriter.IncreaseStock(2);
        staleWriter.IncreaseStock(3);

        await firstContext.SaveChangesAsync();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            () => staleContext.SaveChangesAsync());

        await using InventoryDbContext verificationContext = new(options);
        InventoryItem persisted = await verificationContext.InventoryItems.AsNoTracking().SingleAsync();
        Assert.Equal(12, persisted.QuantityOnHand);
        Assert.Equal(1, persisted.ConcurrencyVersion);
    }
}
