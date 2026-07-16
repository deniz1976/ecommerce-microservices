using ECommerce.Basket.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Basket.Infrastructure.Persistence.Configurations;

public sealed class BasketCheckoutSnapshotConfiguration : IEntityTypeConfiguration<BasketCheckoutSnapshot>
{
    public void Configure(EntityTypeBuilder<BasketCheckoutSnapshot> builder)
    {
        builder.ToTable("basket_checkout_snapshots");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.CustomerId).HasColumnName("customer_id");
        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(x => x.TotalAmount).HasColumnName("total_amount").HasPrecision(18, 2);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.CreatedAt);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.BasketCheckoutSnapshot)
            .HasForeignKey(x => x.BasketCheckoutSnapshotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
