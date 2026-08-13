using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Persistence;
using ECommerce.OrderingSaga.Domain;
using ECommerce.OrderingSaga.Application.Workflows;
using ECommerce.OrderingSaga.Infrastructure.Messaging;
using ECommerce.OrderingSaga.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.OrderingSaga.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingSagaInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresDbContext<OrderingSagaDbContext>(configuration, "OrderingSagaDb");
        services.AddScoped<IRepository<OrderWorkflow, Guid>>(serviceProvider =>
            new EfRepository<OrderWorkflow, Guid>(
                serviceProvider.GetRequiredService<OrderingSagaDbContext>(),
                workflow => workflow.Id));
        services.AddScoped<IUnitOfWork, EfUnitOfWork<OrderingSagaDbContext>>();
        services.AddScoped<IOrderWorkflowIdentityReader, OrderWorkflowIdentityReader>();
        services.AddScoped<IOrderWorkflowTimeoutReader, OrderWorkflowTimeoutReader>();
        services.AddScoped<IWorkflowCommandPublisher, MassTransitWorkflowCommandPublisher>();
        OrderWorkflowTimeoutOptions timeoutOptions = configuration
            .GetSection(OrderWorkflowTimeoutOptions.SectionName)
            .Get<OrderWorkflowTimeoutOptions>() ?? new OrderWorkflowTimeoutOptions();
        ValidateTimeoutOptions(timeoutOptions);
        services.AddSingleton(timeoutOptions);
        return services;
    }

    private static void ValidateTimeoutOptions(OrderWorkflowTimeoutOptions options)
    {
        if (options.InventorySeconds is < 5 or > 3600 ||
            options.PaymentSeconds is < 5 or > 3600 ||
            options.ShippingSeconds is < 5 or > 3600 ||
            options.PollIntervalSeconds is < 1 or > 60 ||
            options.BatchSize is < 1 or > 100)
        {
            throw new InvalidOperationException(
                $"{OrderWorkflowTimeoutOptions.SectionName} configuration is outside the supported bounds.");
        }
    }
}
