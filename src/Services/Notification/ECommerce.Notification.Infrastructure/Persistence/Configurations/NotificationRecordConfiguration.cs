using ECommerce.Notification.Domain;
using ECommerce.BuildingBlocks.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Notification.Infrastructure.Persistence.Configurations;

public sealed class NotificationRecordConfiguration : IEntityTypeConfiguration<NotificationRecord>
{
    public void Configure(EntityTypeBuilder<NotificationRecord> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.SourceMessageId).HasColumnName("source_message_id");
        builder.Property(x => x.CustomerId).HasColumnName("customer_id");
        builder.Property(x => x.OrderId).HasColumnName("order_id");
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(128).IsRequired();
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(256).IsRequired();
        builder.Property(x => x.Message).HasColumnName("message").HasMaxLength(1024).IsRequired();
        builder.Property(x => x.Culture).HasColumnName("culture").HasMaxLength(8).IsRequired();
        builder.Property(x => x.ReasonCode).HasColumnName("reason_code").HasMaxLength(64);
        builder.Property(x => x.TrackingNumber).HasColumnName("tracking_number").HasMaxLength(64);
        builder.Property(x => x.Channel).HasColumnName("channel").HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsUtcTimestamp();
        builder.Property(x => x.ReadAt).HasColumnName("read_at").IsUtcTimestamp();
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => new { x.CustomerId, x.ReadAt, x.CreatedAt });
        builder.HasIndex(x => new { x.SourceMessageId, x.Channel }).IsUnique();
    }
}
