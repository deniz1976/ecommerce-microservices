using ECommerce.BuildingBlocks.Persistence;
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
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        return services;
    }
}
