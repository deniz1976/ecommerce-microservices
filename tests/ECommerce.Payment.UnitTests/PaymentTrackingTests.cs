using ECommerce.Payment.Domain;
using ECommerce.Payment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Payment.UnitTests;

public sealed class PaymentTrackingTests
{
    [Fact]
    public void MarkRefundedTracksClientGeneratedTransactionAsAdded()
    {
        ECommerce.Payment.Domain.Payment payment = ECommerce.Payment.Domain.Payment.CreateAuthorized(
            Guid.NewGuid(),
            Guid.NewGuid(),
            10.50m,
            "USD");

        DbContextOptions<PaymentDbContext> options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseNpgsql("Host=localhost;Database=tracking_test")
            .Options;
        using PaymentDbContext dbContext = new(options);
        dbContext.Attach(payment);

        payment.MarkRefunded(10.50m, "USD", "Shipment failed.");
        dbContext.ChangeTracker.DetectChanges();

        PaymentTransaction refund = Assert.Single(
            payment.Transactions,
            transaction => transaction.Type == PaymentTransactionType.Refund);
        Assert.Equal(EntityState.Added, dbContext.Entry(refund).State);
    }
}
