using ECommerce.BuildingBlocks.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Shipping.Infrastructure.Persistence.Configurations;

public sealed class ShipmentConfiguration : IEntityTypeConfiguration<Domain.Shipment>
{
    public void Configure(EntityTypeBuilder<Domain.Shipment> builder)
    {
        builder.ToTable("shipments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.OrderId).HasColumnName("order_id");
        builder.Property(x => x.CustomerId).HasColumnName("customer_id");
        builder.Property(x => x.RecipientName).HasColumnName("recipient_name").HasMaxLength(256).IsRequired();
        builder.Property(x => x.AddressLine).HasColumnName("address_line").HasMaxLength(512).IsRequired();
        builder.Property(x => x.City).HasColumnName("city").HasMaxLength(128).IsRequired();
        builder.Property(x => x.CountryCode).HasColumnName("country_code").HasMaxLength(2).IsRequired();
        builder.Property(x => x.PostalCode).HasColumnName("postal_code").HasMaxLength(32).IsRequired();
        builder.Property(x => x.TrackingNumber).HasColumnName("tracking_number").HasMaxLength(64);
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.FailureReason).HasColumnName("failure_reason").HasMaxLength(512);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsUtcTimestamp();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsUtcTimestamp();
        builder.HasIndex(x => x.OrderId).IsUnique();
        builder.HasIndex(x => x.TrackingNumber).IsUnique();
    }
}
