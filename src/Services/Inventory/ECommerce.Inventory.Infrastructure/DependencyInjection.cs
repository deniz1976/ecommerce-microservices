using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Persistence;
using ECommerce.Inventory.Domain;
using ECommerce.Inventory.Application.Inventory;
using ECommerce.Inventory.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresDbContext<InventoryDbContext>(configuration, "InventoryDb");
        services.AddScoped<IRepository<InventoryItem, Guid>>(serviceProvider =>
            new EfRepository<InventoryItem, Guid>(
                serviceProvider.GetRequiredService<InventoryDbContext>(),
                item => item.ProductId));
        services.AddScoped<IRepository<StockReservation, Guid>>(serviceProvider =>
            new EfRepository<StockReservation, Guid>(
                serviceProvider.GetRequiredService<InventoryDbContext>(),
                reservation => reservation.Id));
        services.AddScoped<IUnitOfWork, EfUnitOfWork<InventoryDbContext>>();
        services.AddScoped<IStockReservationIdentityReader, StockReservationIdentityReader>();
        return services;
    }
}
