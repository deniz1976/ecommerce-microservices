using ECommerce.Catalog.Domain;
using ECommerce.BuildingBlocks.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Catalog.Infrastructure.Persistence.Configurations;

public sealed class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("stores");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.OwnerUserId).HasColumnName("owner_user_id");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(160).IsRequired();
        builder.Property(x => x.Slug).HasColumnName("slug").HasMaxLength(160).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsUtcTimestamp();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsUtcTimestamp();

        builder.HasIndex(x => x.OwnerUserId);
        builder.HasIndex(x => x.Slug).IsUnique();
    }
}
