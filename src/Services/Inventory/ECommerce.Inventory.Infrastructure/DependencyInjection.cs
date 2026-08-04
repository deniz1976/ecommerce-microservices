using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Persistence;
using ECommerce.Inventory.Domain;
using ECommerce.Inventory.Application.Inventory;
using ECommerce.Inventory.Infrastructure.Catalog;
using ECommerce.Inventory.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ECommerce.Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresDbContext<InventoryDbContext>(configuration, "InventoryDb");
        services.AddScoped<IRepository<InventoryItem, Guid>>(serviceProvider =>
            new EfRepository<InventoryItem, Guid>(
                serviceProvider.GetRequiredService<InventoryDbContext>(),
                item => item.ProductId));
        services.AddScoped<IRepository<StockReservation, Guid>>(serviceProvider =>
            new EfRepository<StockReservation, Guid>(
                serviceProvider.GetRequiredService<InventoryDbContext>(),
                reservation => reservation.Id));
        services.AddScoped<IUnitOfWork, EfUnitOfWork<InventoryDbContext>>();
        services.AddScoped<IStockReservationIdentityReader, StockReservationIdentityReader>();
        services.AddScoped<IInventoryQueryReader, InventoryQueryReader>();
        services.AddOptions<CatalogClientOptions>()
            .Bind(configuration.GetSection(CatalogClientOptions.SectionName))
            .Validate(
                options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out Uri? uri) &&
                    (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps),
                "CatalogClient:BaseUrl must be an absolute HTTP or HTTPS URL.")
            .Validate(
                options => options.TimeoutSeconds is >= 1 and <= 30,
                "CatalogClient:TimeoutSeconds must be between 1 and 30.")
            .ValidateOnStart();
        services.AddHttpClient<IProductInventoryAccessAuthorizer, CatalogProductInventoryAccessAuthorizer>(
            (serviceProvider, client) =>
            {
                CatalogClientOptions options = serviceProvider
                    .GetRequiredService<IOptions<CatalogClientOptions>>()
                    .Value;
                client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });
        return services;
    }
}
