using ECommerce.Basket.Application.Baskets;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Basket.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddBasketApplication(this IServiceCollection services)
    {
        services.AddScoped<BasketService>();
        return services;
    }
}
