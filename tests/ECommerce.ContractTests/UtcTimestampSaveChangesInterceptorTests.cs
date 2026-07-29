using ECommerce.BuildingBlocks.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ECommerce.ContractTests;

public sealed class UtcTimestampSaveChangesInterceptorTests
{
    [Fact]
    public void NormalizeTrackedTimestampsConvertsOnlyAnnotatedApplicationValuesToUtc()
    {
        NpgsqlConnectionStringBuilder connectionString = new()
        {
            Host = "localhost",
            Database = "utc_test",
            Username = "unused",
            Password = "unused"
        };
        DbContextOptions<UtcTimestampTestDbContext> options =
            new DbContextOptionsBuilder<UtcTimestampTestDbContext>()
                .UseNpgsql(connectionString.ConnectionString)
                .Options;
        using UtcTimestampTestDbContext dbContext = new(options);
        UtcTimestampTestEntity entity = new()
        {
            OccurredAt = new DateTimeOffset(2026, 7, 25, 18, 0, 0, TimeSpan.FromHours(3)),
            FrameworkTimestamp = new DateTime(2026, 7, 25, 18, 0, 0, DateTimeKind.Local)
        };
        dbContext.Add(entity);

        UtcTimestampSaveChangesInterceptor.NormalizeTrackedTimestamps(dbContext);

        Assert.Equal(TimeSpan.Zero, entity.OccurredAt.Offset);
        Assert.Equal(DateTimeKind.Local, entity.FrameworkTimestamp.Kind);
    }
}
