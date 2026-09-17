using ECommerce.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Catalog.Infrastructure.Persistence.Configurations;

public sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("product_images");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.ProductId).HasColumnName("product_id");
        builder.Property(x => x.PublicId).HasColumnName("public_id").HasMaxLength(256).IsRequired();
        builder.Property(x => x.Url).HasColumnName("url").HasMaxLength(2048).IsRequired();
        builder.Property(x => x.SecureUrl).HasColumnName("secure_url").HasMaxLength(2048).IsRequired();
        builder.Property(x => x.Width).HasColumnName("width");
        builder.Property(x => x.Height).HasColumnName("height");
        builder.Property(x => x.Format).HasColumnName("format").HasMaxLength(32).IsRequired();
        builder.Property(x => x.SortOrder).HasColumnName("sort_order");
        builder.Property(x => x.IsMain).HasColumnName("is_main");

        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => new { x.ProductId, x.SortOrder }).IsUnique();
    }
}
