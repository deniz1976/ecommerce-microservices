using ECommerce.BuildingBlocks.Persistence;
using ECommerce.Ordering.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Ordering.Infrastructure.Persistence.Configurations;

public sealed class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("order_status_history");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.OrderId).HasColumnName("order_id");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.OccurredAt).HasColumnName("occurred_at").IsUtcTimestamp();
        builder.Property(x => x.ReasonCode).HasColumnName("reason_code").HasMaxLength(64);

        builder.HasIndex(x => new { x.OrderId, x.OccurredAt });
    }
}
