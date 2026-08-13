using ECommerce.Inventory.Application.Inventory;
using ECommerce.Inventory.Domain;

namespace ECommerce.ContractTests;

public sealed class InventoryReservationInvariantTests
{
    [Fact]
    public async Task DuplicateProductLinesAreValidatedAsOneRequestedQuantity()
    {
        Guid productId = Guid.NewGuid();
        InventoryItem item = new(productId, 5);
        item.DequeuePendingMovements();
        FakeInventoryPersistence persistence = new(item);
        InventoryService service = new(
            persistence,
            persistence,
            persistence,
            new EmptyStockReservationIdentityReader(),
            new GrantedProductInventoryAccessAuthorizer());
        InventoryReservationRequest request = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            [
                new InventoryReservationRequestItem(productId, 3),
                new InventoryReservationRequestItem(productId, 3)
            ]);

        InventoryReservationResult result = await service.ReserveAsync(
            request,
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(0, item.ReservedQuantity);
        Assert.Empty(item.PendingMovements);
        Assert.Single(persistence.Reservations);
        Assert.Equal(1, persistence.SaveChangesCount);
    }
}
