using ECommerce.Ordering.Application.Orders;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Ordering.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingApplication(this IServiceCollection services)
    {
        services.AddScoped<OrderService>();
        services.AddScoped<OrderStatusService>();
        return services;
    }
}
