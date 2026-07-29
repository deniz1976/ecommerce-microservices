namespace ECommerce.BuildingBlocks.Contracts.Persistence;

public interface IRepository<TEntity, in TKey>
    where TEntity : class
{
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken);

    void Add(TEntity entity);
}
