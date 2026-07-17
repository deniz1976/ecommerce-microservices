using ECommerce.BuildingBlocks.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.Inventory.Infrastructure.Persistence;

public sealed class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<InventoryDbContext> builder = new();

        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__InventoryDb")
            ?? PostgresConnectionString.CreateLocalDevelopment("inventory_db", "inventory_user");

        builder.UseNpgsql(PostgresConnectionString.Normalize(connectionString));

        return new InventoryDbContext(builder.Options);
    }
}
