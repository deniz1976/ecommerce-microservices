using ECommerce.BuildingBlocks.Persistence;
using ECommerce.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Catalog.Infrastructure.Persistence.Configurations;

public sealed class ProductImageDeletionJobConfiguration
    : IEntityTypeConfiguration<ProductImageDeletionJob>
{
    public void Configure(EntityTypeBuilder<ProductImageDeletionJob> builder)
    {
        builder.ToTable("product_image_deletion_jobs");
        builder.HasKey(job => job.Id);
        builder.Property(job => job.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(job => job.PublicId).HasColumnName("public_id").HasMaxLength(256).IsRequired();
        builder.Property(job => job.AttemptCount).HasColumnName("attempt_count").IsRequired();
        builder.Property(job => job.CreatedAt).HasColumnName("created_at").IsUtcTimestamp();
        builder.Property(job => job.NextAttemptAt).HasColumnName("next_attempt_at").IsUtcTimestamp();
        builder.HasIndex(job => job.PublicId).IsUnique();
        builder.HasIndex(job => job.NextAttemptAt);
    }
}
