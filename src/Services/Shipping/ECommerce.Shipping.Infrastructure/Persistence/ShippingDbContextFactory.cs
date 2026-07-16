using ECommerce.BuildingBlocks.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.Shipping.Infrastructure.Persistence;

public sealed class ShippingDbContextFactory : IDesignTimeDbContextFactory<ShippingDbContext>
{
    public ShippingDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<ShippingDbContext> builder = new();

        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__ShippingDb")
            ?? "Host=localhost;Port=5432;Database=shipping_db;Username=shipping_user;Password=shipping_password";

        builder.UseNpgsql(PostgresConnectionString.Normalize(connectionString));

        return new ShippingDbContext(builder.Options);
    }
}
