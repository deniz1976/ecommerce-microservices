using ECommerce.Inventory.Application.Inventory;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Inventory.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryApplication(this IServiceCollection services)
    {
        services.AddScoped<InventoryService>();
        return services;
    }
}
