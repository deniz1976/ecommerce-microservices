using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Persistence;
using ECommerce.Shipping.Application.Shipments;
using ECommerce.Shipping.Infrastructure.Persistence;
using ECommerce.Shipping.Infrastructure.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Shipping.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddShippingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresDbContext<ShippingDbContext>(configuration, "ShippingDb");
        services.AddScoped<IRepository<Domain.Shipment, Guid>>(serviceProvider =>
            new EfRepository<Domain.Shipment, Guid>(
                serviceProvider.GetRequiredService<ShippingDbContext>(),
                shipment => shipment.Id));
        services.AddScoped<IUnitOfWork, EfUnitOfWork<ShippingDbContext>>();
        services.AddScoped<IShipmentIdentityReader, ShipmentIdentityReader>();
        services.AddScoped<IShipmentQueryReader, ShipmentQueryReader>();
        services.Configure<MockShippingProviderOptions>(
            configuration.GetSection(MockShippingProviderOptions.SectionName));
        services.AddSingleton<IShippingProvider, MockShippingProvider>();
        return services;
    }
}
