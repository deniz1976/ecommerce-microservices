using ECommerce.BuildingBlocks.Persistence;
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
        services.AddScoped<IOrderWorkflowRepository, OrderWorkflowRepository>();
        services.AddScoped<IWorkflowCommandPublisher, MassTransitWorkflowCommandPublisher>();
        return services;
    }
}
