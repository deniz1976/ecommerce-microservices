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
            ?? "Host=localhost;Port=5432;Database=inventory_db;Username=inventory_user;Password=inventory_password";

        builder.UseNpgsql(PostgresConnectionString.Normalize(connectionString));

        return new InventoryDbContext(builder.Options);
    }
}
