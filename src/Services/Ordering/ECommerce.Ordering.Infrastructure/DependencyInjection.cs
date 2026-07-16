using ECommerce.BuildingBlocks.Persistence;
using ECommerce.Ordering.Application.Orders;
using ECommerce.Ordering.Infrastructure.Messaging;
using ECommerce.Ordering.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Ordering.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresDbContext<OrderingDbContext>(configuration, "OrderingDb");
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderSubmittedPublisher, MassTransitOrderSubmittedPublisher>();
        return services;
    }
}
