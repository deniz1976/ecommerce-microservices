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
            ?? "Host=localhost;Port=5432;Database=payment_db;Username=payment_user;Password=payment_password";

        builder.UseNpgsql(PostgresConnectionString.Normalize(connectionString));

        return new PaymentDbContext(builder.Options);
    }
}
