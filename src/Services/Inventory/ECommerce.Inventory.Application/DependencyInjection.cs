using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Inventory.Application.Commands.ReleaseInventory;
using ECommerce.Inventory.Application.Commands.ReserveInventory;
using ECommerce.Inventory.Application.Commands.UpsertInventoryItem;
using ECommerce.Inventory.Application.Inventory;
using ECommerce.Inventory.Application.Queries.GetInventoryItem;
using ECommerce.Inventory.Application.Queries.SearchManagedInventory;
using ECommerce.Inventory.Application.Queries.SearchStockMovements;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Inventory.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddScoped<InventoryReservationService>();
        services.AddScoped<InventoryReleaseService>();
        services.AddScoped<StockReservationLoader>();
        services.AddScoped<InventoryManagementService>();
        services.AddScoped<InventoryQueryService>();
        services.AddScoped<StockMovementQueryService>();
        services.AddScoped<
            IQueryHandler<GetInventoryItemQuery, InventoryItemResponse?>,
            GetInventoryItemQueryHandler>();
        services.AddScoped<
            IQueryHandler<SearchManagedInventoryQuery, PagedResult<InventoryItemResponse>>,
            SearchManagedInventoryQueryHandler>();
        services.AddScoped<
            IQueryHandler<SearchStockMovementsQuery, PagedResult<StockMovementResponse>>,
            SearchStockMovementsQueryHandler>();
        services.AddScoped<
            ICommandHandler<UpsertInventoryItemCommand, Result<InventoryItemResponse>>,
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
