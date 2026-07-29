using ECommerce.BuildingBlocks.Persistence;
using ECommerce.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Identity.Infrastructure.Persistence.Configurations;

public sealed class RoleReconciliationJobConfiguration
    : IEntityTypeConfiguration<RoleReconciliationJob>
{
    public void Configure(EntityTypeBuilder<RoleReconciliationJob> builder)
    {
        builder.ToTable("role_reconciliation_jobs");
        builder.HasKey(job => job.Id);
        builder.Property(job => job.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(job => job.ExternalSubject).HasColumnName("external_subject").HasMaxLength(256).IsRequired();
        builder.Property(job => job.DesiredRole).HasColumnName("desired_role").HasMaxLength(128).IsRequired();
        builder.Property(job => job.CurrentExternalRole).HasColumnName("current_external_role").HasMaxLength(128);
        builder.Property(job => job.AttemptCount).HasColumnName("attempt_count").IsRequired();
        builder.Property(job => job.CreatedAt).HasColumnName("created_at").IsUtcTimestamp();
        builder.Property(job => job.NextAttemptAt).HasColumnName("next_attempt_at").IsUtcTimestamp();
        builder.Property(job => job.CompletedAt).HasColumnName("completed_at").IsUtcTimestamp();
        builder.HasIndex(job => new { job.ExternalSubject, job.DesiredRole })
            .IsUnique()
            .HasFilter("completed_at IS NULL");
        builder.HasIndex(job => new { job.CompletedAt, job.NextAttemptAt });
    }
}
