using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ECommerce.BuildingBlocks.Persistence;

namespace ECommerce.Catalog.Infrastructure.Persistence;

public sealed class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<CatalogDbContext> builder = new();

        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__CatalogDb")
            ?? "Host=localhost;Port=5432;Database=catalog_db;Username=catalog_user;Password=catalog_password";

        builder.UseNpgsql(PostgresConnectionString.Normalize(connectionString));

        return new CatalogDbContext(builder.Options);
    }
}
