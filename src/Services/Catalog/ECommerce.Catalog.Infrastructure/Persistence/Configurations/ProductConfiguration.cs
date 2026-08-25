using ECommerce.Catalog.Domain;
using ECommerce.BuildingBlocks.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Catalog.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Sku).HasColumnName("sku").HasMaxLength(128).IsRequired();
        builder.Property(x => x.CategoryId).HasColumnName("category_id");
        builder.Property(x => x.BrandId).HasColumnName("brand_id");
        builder.Property(x => x.StoreId).HasColumnName("store_id");
        builder.Property(x => x.Price).HasColumnName("price").HasPrecision(18, 2);
        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsUtcTimestamp();
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsUtcTimestamp()
            .IsConcurrencyToken();

        builder.HasIndex(x => x.Sku).IsUnique();
        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.BrandId);
        builder.HasIndex(x => x.StoreId);
        builder.HasIndex(x => x.Status);

        builder.HasMany(x => x.Translations)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Images)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Store)
            .WithMany()
            .HasForeignKey(x => x.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.Translations)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
        builder.Navigation(x => x.Images)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
        builder.Navigation(x => x.Category).AutoInclude();
        builder.Navigation(x => x.Brand).AutoInclude();
    }
}
