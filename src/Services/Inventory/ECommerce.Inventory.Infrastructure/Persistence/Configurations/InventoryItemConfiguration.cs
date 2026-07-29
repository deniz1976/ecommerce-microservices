using ECommerce.Inventory.Domain;
using ECommerce.BuildingBlocks.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Inventory.Infrastructure.Persistence.Configurations;

public sealed class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("inventory_items");
        builder.HasKey(x => x.ProductId);

        builder.Property(x => x.ProductId).HasColumnName("product_id");
        builder.Property(x => x.QuantityOnHand).HasColumnName("quantity_on_hand");
        builder.Property(x => x.ReservedQuantity).HasColumnName("reserved_quantity");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsUtcTimestamp();

        builder.Ignore(x => x.AvailableQuantity);
    }
}
