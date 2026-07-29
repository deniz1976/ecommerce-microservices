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
        services.AddScoped<IWorkflowCommandPublisher, MassTransitWorkflowCommandPublisher>();
        return services;
    }
}
