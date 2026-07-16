using ECommerce.Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Inventory.Infrastructure.Persistence.Configurations;

public sealed class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.ToTable("stock_reservations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.OrderId).HasColumnName("order_id");
        builder.Property(x => x.ProductId).HasColumnName("product_id");
        builder.Property(x => x.Quantity).HasColumnName("quantity");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.FailureReason).HasColumnName("failure_reason").HasMaxLength(1024);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => new { x.OrderId, x.ProductId }).IsUnique();
        builder.HasIndex(x => x.Status);
    }
}
