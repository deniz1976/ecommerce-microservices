using ECommerce.OrderingSaga.Domain;
using ECommerce.BuildingBlocks.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.OrderingSaga.Infrastructure.Persistence.Configurations;

public sealed class OrderWorkflowConfiguration : IEntityTypeConfiguration<OrderWorkflow>
{
    public void Configure(EntityTypeBuilder<OrderWorkflow> builder)
    {
        builder.ToTable("order_workflows");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.OrderId).HasColumnName("order_id");
        builder.Property(x => x.CustomerId).HasColumnName("customer_id");
        builder.Property(x => x.TotalAmount).HasColumnName("total_amount").HasPrecision(18, 2);
        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(x => x.RecipientName).HasColumnName("recipient_name").HasMaxLength(256).IsRequired();
        builder.Property(x => x.AddressLine).HasColumnName("address_line").HasMaxLength(512).IsRequired();
        builder.Property(x => x.City).HasColumnName("city").HasMaxLength(128).IsRequired();
        builder.Property(x => x.CountryCode).HasColumnName("country_code").HasMaxLength(2).IsRequired();
        builder.Property(x => x.PostalCode).HasColumnName("postal_code").HasMaxLength(32).IsRequired();
        builder.Property(x => x.CorrelationId).HasColumnName("correlation_id");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(64);
        builder.Property(x => x.CancellationReason).HasColumnName("cancellation_reason").HasMaxLength(512);
        builder.Property(x => x.StepDeadlineAt).HasColumnName("step_deadline_at").IsUtcTimestamp();
        builder.Property(x => x.TimeoutHandledAt).HasColumnName("timeout_handled_at").IsUtcTimestamp();
        builder.Property(x => x.ConcurrencyVersion)
            .HasColumnName("concurrency_version")
            .IsConcurrencyToken();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsUtcTimestamp();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsUtcTimestamp();
        builder.HasIndex(x => x.OrderId).IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.Status, x.StepDeadlineAt });
        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
    }
}
