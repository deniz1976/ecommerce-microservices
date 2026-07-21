using ECommerce.Catalog.Application.Products;
using ECommerce.Catalog.Application.References;
using ECommerce.Catalog.Application.Stores;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Catalog.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogApplication(this IServiceCollection services)
    {
        services.AddScoped<ProductService>();
        services.AddScoped<IProductStoreAccessValidator, ProductStoreAccessValidator>();
        services.AddScoped<IProductReferenceValidator, ProductReferenceValidator>();
        services.AddScoped<IProductImageAttacher, ProductImageAttacher>();
        services.AddScoped<StoreService>();
        services.AddScoped<CatalogReferenceService>();
        return services;
    }
}
