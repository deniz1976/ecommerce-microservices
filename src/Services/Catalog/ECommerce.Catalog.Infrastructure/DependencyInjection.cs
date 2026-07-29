using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Persistence;
using ECommerce.Catalog.Application.Images;
using ECommerce.Catalog.Application.Metrics;
using ECommerce.Catalog.Application.Products;
using ECommerce.Catalog.Application.References;
using ECommerce.Catalog.Application.Stores;
using ECommerce.Catalog.Infrastructure.Images;
using ECommerce.Catalog.Infrastructure.Persistence;
using ECommerce.Catalog.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresDbContext<CatalogDbContext>(configuration, "CatalogDb");
        services.Configure<CloudinaryOptions>(configuration.GetSection(CloudinaryOptions.SectionName));
        services.AddScoped<IRepository<Product, Guid>>(serviceProvider =>
            new EfRepository<Product, Guid>(
                serviceProvider.GetRequiredService<CatalogDbContext>(),
                product => product.Id));
        services.AddScoped<IRepository<Store, Guid>>(serviceProvider =>
            new EfRepository<Store, Guid>(
                serviceProvider.GetRequiredService<CatalogDbContext>(),
                store => store.Id));
        services.AddScoped<IUnitOfWork, EfUnitOfWork<CatalogDbContext>>();
        services.AddScoped<IProductImageDeletionQueue, ProductImageDeletionQueue>();
        services.AddScoped<ProductImageDeletionProcessor>();
        services.AddSingleton<ProductImageDeletionMetrics>();
        services.AddSingleton(TimeProvider.System);
        services.AddHostedService<ProductImageDeletionBackgroundService>();
        services.AddScoped<ProductReader>();
        services.AddScoped<IProductSearchReader>(
            serviceProvider => serviceProvider.GetRequiredService<ProductReader>());
        services.AddScoped<IProductReferenceReader>(
            serviceProvider => serviceProvider.GetRequiredService<ProductReader>());
        services.AddScoped<ICatalogMetricsReader, CatalogMetricsReader>();
        services.AddScoped<IStoreReader, StoreReader>();
        services.AddScoped<ICatalogReferenceReader, CatalogReferenceReader>();
        services.AddHttpClient<IProductImageStorage, CloudImageService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        return services;
    }
}
