using ECommerce.Ordering.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Ordering.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.OrderId).HasColumnName("order_id");
        builder.Property(x => x.ProductId).HasColumnName("product_id");
        builder.Property(x => x.StoreId).HasColumnName("store_id");
        builder.Property(x => x.ProductName).HasColumnName("product_name").HasMaxLength(256).IsRequired();
        builder.Property(x => x.Quantity).HasColumnName("quantity");
        builder.Property(x => x.UnitPrice).HasColumnName("unit_price").HasPrecision(18, 2);
        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();

        builder.Ignore(x => x.TotalPrice);
        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => new { x.StoreId, x.OrderId });
    }
}
