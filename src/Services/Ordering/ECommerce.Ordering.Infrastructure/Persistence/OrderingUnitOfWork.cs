using ECommerce.BuildingBlocks.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Ordering.Infrastructure.Persistence;

public sealed class OrderingUnitOfWork : IUnitOfWork
{
    private readonly OrderingDbContext dbContext;
    private readonly ILogger<OrderingUnitOfWork> logger;

    public OrderingUnitOfWork(
        OrderingDbContext dbContext,
        ILogger<OrderingUnitOfWork> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            string entries = string.Join(
                ", ",
                exception.Entries.Select(
                    entry => $"{entry.Metadata.ClrType.FullName}:{entry.State}"));
            logger.LogError(
                exception,
                "Ordering concurrency conflict affected tracked entries: {Entries}.",
                entries);
            throw;
        }
    }
}
