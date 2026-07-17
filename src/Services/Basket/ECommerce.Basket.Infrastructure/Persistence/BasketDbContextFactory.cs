using ECommerce.BuildingBlocks.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.Basket.Infrastructure.Persistence;

public sealed class BasketDbContextFactory : IDesignTimeDbContextFactory<BasketDbContext>
{
    public BasketDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<BasketDbContext> builder = new();

        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__BasketDb")
            ?? PostgresConnectionString.CreateLocalDevelopment("basket_db", "basket_user");

        builder.UseNpgsql(PostgresConnectionString.Normalize(connectionString));

        return new BasketDbContext(builder.Options);
    }
}
