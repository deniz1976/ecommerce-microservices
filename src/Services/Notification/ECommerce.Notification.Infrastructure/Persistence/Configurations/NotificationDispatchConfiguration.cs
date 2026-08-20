using ECommerce.BuildingBlocks.Persistence;
using ECommerce.Notification.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Notification.Infrastructure.Persistence.Configurations;

public sealed class NotificationDispatchConfiguration : IEntityTypeConfiguration<NotificationDispatch>
{
    public void Configure(EntityTypeBuilder<NotificationDispatch> builder)
    {
        builder.ToTable("notification_dispatches");
        builder.HasKey(dispatch => dispatch.Id);
        builder.Property(dispatch => dispatch.Id).HasColumnName("id");
        builder.Property(dispatch => dispatch.NotificationId).HasColumnName("notification_id");
        builder.Property(dispatch => dispatch.Channel).HasColumnName("channel").HasConversion<string>().HasMaxLength(16);
        builder.Property(dispatch => dispatch.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(16);
        builder.Property(dispatch => dispatch.AttemptCount).HasColumnName("attempt_count");
        builder.Property(dispatch => dispatch.CreatedAt).HasColumnName("created_at").IsUtcTimestamp();
        builder.Property(dispatch => dispatch.NextAttemptAt).HasColumnName("next_attempt_at").IsUtcTimestamp();
        builder.Property(dispatch => dispatch.DeliveredAt).HasColumnName("delivered_at").IsUtcTimestamp();
        builder.HasIndex(dispatch => new { dispatch.NotificationId, dispatch.Channel }).IsUnique();
        builder.HasIndex(dispatch => new { dispatch.Status, dispatch.NextAttemptAt });
        builder.HasOne<NotificationRecord>()
            .WithMany()
            .HasForeignKey(dispatch => dispatch.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
