using ECommerce.BuildingBlocks.Persistence;
using ECommerce.Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Inventory.Infrastructure.Persistence.Configurations;

public sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("stock_movements");
        builder.HasKey(movement => movement.Id);
        builder.Property(movement => movement.Id).HasColumnName("id");
        builder.Property(movement => movement.ProductId).HasColumnName("product_id");
        builder.Property(movement => movement.Type)
            .HasColumnName("movement_type")
            .HasConversion<string>()
            .HasMaxLength(32);
        builder.Property(movement => movement.Quantity).HasColumnName("quantity");
        builder.Property(movement => movement.QuantityOnHandBefore)
            .HasColumnName("quantity_on_hand_before");
        builder.Property(movement => movement.QuantityOnHandAfter)
            .HasColumnName("quantity_on_hand_after");
        builder.Property(movement => movement.ReservedQuantityBefore)
            .HasColumnName("reserved_quantity_before");
        builder.Property(movement => movement.ReservedQuantityAfter)
            .HasColumnName("reserved_quantity_after");
        builder.Property(movement => movement.OrderId).HasColumnName("order_id");
        builder.Property(movement => movement.ReservationId).HasColumnName("reservation_id");
        builder.Property(movement => movement.OccurredAt)
            .HasColumnName("occurred_at")
            .IsUtcTimestamp();

        builder.HasIndex(movement => new { movement.ProductId, movement.OccurredAt });
        builder.HasIndex(movement => movement.OrderId);
        builder.HasIndex(movement => new { movement.Type, movement.ReservationId })
            .IsUnique()
            .HasFilter("reservation_id IS NOT NULL");
    }
}
