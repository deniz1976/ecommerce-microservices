using ECommerce.Shipping.Application.Shipments;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Shipping.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddShippingApplication(this IServiceCollection services)
    {
        services.AddScoped<ShipmentService>();
        return services;
    }
}
