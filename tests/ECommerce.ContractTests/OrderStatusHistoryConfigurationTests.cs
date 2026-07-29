using ECommerce.Ordering.Domain;
using ECommerce.Ordering.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ECommerce.ContractTests;

public sealed class OrderStatusHistoryConfigurationTests
{
    [Fact]
    public void StatusHistoryIdUsesDomainGeneratedValue()
    {
        DbContextOptions<OrderingDbContext> options =
            new DbContextOptionsBuilder<OrderingDbContext>()
                .UseNpgsql("Host=localhost;Database=model_check")
                .Options;
        using OrderingDbContext dbContext = new(options);

        IProperty idProperty = dbContext.Model
            .FindEntityType(typeof(OrderStatusHistory))!
            .FindProperty(nameof(OrderStatusHistory.Id))!;

        Assert.Equal(ValueGenerated.Never, idProperty.ValueGenerated);
    }
}
