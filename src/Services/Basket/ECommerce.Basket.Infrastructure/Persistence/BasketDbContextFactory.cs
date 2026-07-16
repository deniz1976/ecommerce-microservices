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
            ?? "Host=localhost;Port=5432;Database=basket_db;Username=basket_user;Password=basket_password";

        builder.UseNpgsql(PostgresConnectionString.Normalize(connectionString));

        return new BasketDbContext(builder.Options);
    }
}
