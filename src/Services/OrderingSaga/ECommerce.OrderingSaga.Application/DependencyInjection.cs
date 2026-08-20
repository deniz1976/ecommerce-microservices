using ECommerce.OrderingSaga.Application.Workflows;
using Microsoft.Extensions.DependencyInjection;
using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.OrderingSaga.Application.Diagnostics;
using ECommerce.OrderingSaga.Application.Queries.SearchOrderWorkflowDiagnostics;

namespace ECommerce.OrderingSaga.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingSagaApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddScoped<OrderWorkflowLoader>();
        services.AddScoped<OrderWorkflowDiagnosticsService>();
        services.AddScoped<
            IQueryHandler<SearchOrderWorkflowDiagnosticsQuery, PagedResult<OrderWorkflowDiagnosticsResponse>>,
            SearchOrderWorkflowDiagnosticsQueryHandler>();
        services.AddScoped<OrderSubmissionWorkflowService>();
        services.AddScoped<InventoryWorkflowService>();
        services.AddScoped<PaymentWorkflowService>();
        services.AddScoped<ShippingWorkflowService>();
        services.AddScoped<OrderWorkflowCancellationService>();
        services.AddScoped<OrderWorkflowTimeoutService>();
        return services;
    }
}
