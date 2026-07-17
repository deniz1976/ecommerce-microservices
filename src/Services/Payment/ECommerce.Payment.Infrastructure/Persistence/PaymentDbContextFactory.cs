using ECommerce.BuildingBlocks.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.Payment.Infrastructure.Persistence;

public sealed class PaymentDbContextFactory : IDesignTimeDbContextFactory<PaymentDbContext>
{
    public PaymentDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<PaymentDbContext> builder = new();

        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PaymentDb")
            ?? PostgresConnectionString.CreateLocalDevelopment("payment_db", "payment_user");

        builder.UseNpgsql(PostgresConnectionString.Normalize(connectionString));

        return new PaymentDbContext(builder.Options);
    }
}
