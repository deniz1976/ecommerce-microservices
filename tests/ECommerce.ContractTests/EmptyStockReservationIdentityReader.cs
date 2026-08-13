using ECommerce.Inventory.Application.Inventory;

namespace ECommerce.ContractTests;

internal sealed class EmptyStockReservationIdentityReader : IStockReservationIdentityReader
{
    public Task<IReadOnlyCollection<Guid>> FindIdsByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyCollection<Guid>>([]);
    }
}
