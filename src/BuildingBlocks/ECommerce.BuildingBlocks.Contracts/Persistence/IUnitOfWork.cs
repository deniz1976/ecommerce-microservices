namespace ECommerce.BuildingBlocks.Contracts.Persistence;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
