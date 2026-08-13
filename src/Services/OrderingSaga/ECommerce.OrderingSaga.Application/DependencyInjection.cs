using ECommerce.OrderingSaga.Application.Workflows;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.OrderingSaga.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingSagaApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddScoped<OrderWorkflowLoader>();
        services.AddScoped<OrderSubmissionWorkflowService>();
        services.AddScoped<InventoryWorkflowService>();
        services.AddScoped<PaymentWorkflowService>();
        services.AddScoped<ShippingWorkflowService>();
        services.AddScoped<OrderWorkflowCancellationService>();
        services.AddScoped<OrderWorkflowTimeoutService>();
        return services;
    }
}
