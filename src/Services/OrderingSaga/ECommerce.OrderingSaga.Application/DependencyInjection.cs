using ECommerce.OrderingSaga.Application.Workflows;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.OrderingSaga.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingSagaApplication(this IServiceCollection services)
    {
        services.AddScoped<OrderWorkflowService>();
        return services;
    }
}
