using ECommerce.BuildingBlocks.Persistence;
using ECommerce.Catalog.Application.Images;
using ECommerce.Catalog.Application.Products;
using ECommerce.Catalog.Application.References;
using ECommerce.Catalog.Application.Stores;
using ECommerce.Catalog.Infrastructure.Images;
using ECommerce.Catalog.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresDbContext<CatalogDbContext>(configuration, "CatalogDb");
        services.Configure<CloudinaryOptions>(configuration.GetSection(CloudinaryOptions.SectionName));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<ICatalogReferenceRepository, CatalogReferenceRepository>();
        services.AddScoped<ICloudImageService, CloudImageService>();
        return services;
    }
}
