using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.Inventory.Domain;

namespace ECommerce.Inventory.Application.Inventory;

public sealed class StockReservationLoader(
    IRepository<StockReservation, Guid> reservationRepository,
    IStockReservationIdentityReader reservationIdentityReader)
{
    public async Task<IReadOnlyCollection<StockReservation>> LoadByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Guid> reservationIds =
            await reservationIdentityReader.FindIdsByOrderIdAsync(orderId, cancellationToken);
        List<StockReservation> reservations = new(reservationIds.Count);
        foreach (Guid reservationId in reservationIds)
        {
            StockReservation? reservation = await reservationRepository.GetByIdAsync(
                reservationId,
                cancellationToken);
            if (reservation is not null)
            {
                reservations.Add(reservation);
            }
        }

        return reservations;
    }
}
