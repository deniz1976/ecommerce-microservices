using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Inventory.Application.Commands.ReleaseInventory;
using ECommerce.Inventory.Application.Commands.ReserveInventory;
using ECommerce.Inventory.Application.Commands.UpsertInventoryItem;
using ECommerce.Inventory.Application.Inventory;
using ECommerce.Inventory.Application.Queries.GetInventoryItem;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Inventory.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryApplication(this IServiceCollection services)
    {
        services.AddScoped<InventoryService>();
        services.AddScoped<
            IQueryHandler<GetInventoryItemQuery, InventoryItemResponse?>,
            GetInventoryItemQueryHandler>();
        services.AddScoped<
            ICommandHandler<UpsertInventoryItemCommand, InventoryItemResponse>,
            UpsertInventoryItemCommandHandler>();
        services.AddScoped<
            ICommandHandler<ReserveInventoryCommand, InventoryReservationResult>,
            ReserveInventoryCommandHandler>();
        services.AddScoped<
            ICommandHandler<ReleaseInventoryCommand>,
            ReleaseInventoryCommandHandler>();
        return services;
    }
}
