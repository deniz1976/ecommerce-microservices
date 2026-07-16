using ECommerce.Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Catalog.Infrastructure.Persistence;

public sealed class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<ProductTranslation> ProductTranslations => Set<ProductTranslation>();

    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<CategoryTranslation> CategoryTranslations => Set<CategoryTranslation>();

    public DbSet<Brand> Brands => Set<Brand>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
    }
}
