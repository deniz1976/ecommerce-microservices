using ECommerce.BuildingBlocks.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.BuildingBlocks.Persistence;

public sealed class EfUnitOfWork<TDbContext> : IUnitOfWork
    where TDbContext : DbContext
{
    private readonly TDbContext dbContext;

    public EfUnitOfWork(TDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
