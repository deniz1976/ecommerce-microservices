using System.Linq.Expressions;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.BuildingBlocks.Persistence;

public sealed class EfRepository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class
{
    private readonly DbContext dbContext;
    private readonly Expression<Func<TEntity, TKey>> keySelector;

    public EfRepository(
        DbContext dbContext,
        Expression<Func<TEntity, TKey>> keySelector)
    {
        this.dbContext = dbContext;
        this.keySelector = keySelector;
    }

    public Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken)
    {
        ParameterExpression parameter = keySelector.Parameters[0];
        BinaryExpression equality = Expression.Equal(
            keySelector.Body,
            Expression.Constant(id, typeof(TKey)));
        Expression<Func<TEntity, bool>> predicate = Expression.Lambda<Func<TEntity, bool>>(
            equality,
            parameter);

        return dbContext.Set<TEntity>().SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public void Add(TEntity entity)
    {
        dbContext.Set<TEntity>().Add(entity);
    }
}
