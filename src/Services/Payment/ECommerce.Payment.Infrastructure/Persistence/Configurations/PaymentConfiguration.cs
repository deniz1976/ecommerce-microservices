using ECommerce.BuildingBlocks.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Payment.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Domain.Payment>
{
    public void Configure(EntityTypeBuilder<Domain.Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.OrderId).HasColumnName("order_id");
        builder.Property(x => x.CustomerId).HasColumnName("customer_id");
        builder.Property(x => x.Amount).HasColumnName("amount").HasPrecision(18, 2);
        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.ProviderName).HasColumnName("provider_name").HasMaxLength(64);
        builder.Property(x => x.ProviderPaymentReference).HasColumnName("provider_payment_reference").HasMaxLength(256);
        builder.Property(x => x.FailureReason).HasColumnName("failure_reason").HasMaxLength(512);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsUtcTimestamp();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsUtcTimestamp();
        builder.HasIndex(x => x.OrderId).IsUnique();
        builder.HasMany(x => x.Transactions)
            .WithOne()
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Transactions)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
    }
}
