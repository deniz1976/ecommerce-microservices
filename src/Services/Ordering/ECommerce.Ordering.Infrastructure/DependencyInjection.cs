using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Persistence;
using ECommerce.Ordering.Domain;
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
        services.AddScoped<IRepository<Order, Guid>>(serviceProvider =>
            new EfRepository<Order, Guid>(
                serviceProvider.GetRequiredService<OrderingDbContext>(),
                order => order.Id));
        services.AddScoped<IUnitOfWork, OrderingUnitOfWork>();
        services.AddScoped<IOrderReader, OrderReader>();
        services.AddScoped<IOrderSubmittedPublisher, MassTransitOrderSubmittedPublisher>();
        services.AddScoped<
            IOrderCancellationRequestedPublisher,
            MassTransitOrderCancellationRequestedPublisher>();
        return services;
    }
}
