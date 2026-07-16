using ECommerce.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Catalog.Infrastructure.Persistence.Configurations;

public sealed class CategoryTranslationConfiguration : IEntityTypeConfiguration<CategoryTranslation>
{
    public void Configure(EntityTypeBuilder<CategoryTranslation> builder)
    {
        builder.ToTable("category_translations");
        builder.HasKey(x => new { x.CategoryId, x.LanguageCode });

        builder.Property(x => x.CategoryId).HasColumnName("category_id");
        builder.Property(x => x.LanguageCode).HasColumnName("language_code").HasMaxLength(2).IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(256).IsRequired();
    }
}
