using ECommerce.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Catalog.Infrastructure.Persistence.Configurations;

public sealed class ProductTranslationConfiguration : IEntityTypeConfiguration<ProductTranslation>
{
    public void Configure(EntityTypeBuilder<ProductTranslation> builder)
    {
        builder.ToTable("product_translations");
        builder.HasKey(x => new { x.ProductId, x.LanguageCode });

        builder.Property(x => x.ProductId).HasColumnName("product_id");
        builder.Property(x => x.LanguageCode).HasColumnName("language_code").HasMaxLength(2).IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(256).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(4000).IsRequired();
    }
}
