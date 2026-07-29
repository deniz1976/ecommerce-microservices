using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Commands.CreateProduct;
using ECommerce.Catalog.Application.Commands.CreateStore;
using ECommerce.Catalog.Application.Commands.ManageProductImage;
using ECommerce.Catalog.Application.Commands.UpdateProduct;
using ECommerce.Catalog.Application.Metrics;
using ECommerce.Catalog.Application.Products;
using ECommerce.Catalog.Application.Queries.GetCatalogBrands;
using ECommerce.Catalog.Application.Queries.GetCatalogCategories;
using ECommerce.Catalog.Application.Queries.GetCatalogMetrics;
using ECommerce.Catalog.Application.Queries.GetProduct;
using ECommerce.Catalog.Application.Queries.GetStoreById;
using ECommerce.Catalog.Application.Queries.GetStoresByOwner;
using ECommerce.Catalog.Application.Queries.SearchProducts;
using ECommerce.Catalog.Application.References;
using ECommerce.Catalog.Application.Stores;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Catalog.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogApplication(this IServiceCollection services)
    {
        services.AddScoped<ProductService>();
        services.AddScoped<CatalogMetricsService>();
        services.AddScoped<ProductImageService>();
        services.AddScoped<IProductStoreAccessValidator, ProductStoreAccessValidator>();
        services.AddScoped<IProductReferenceValidator, ProductReferenceValidator>();
        services.AddScoped<ProductImageUploadValidator>();
        services.AddScoped<StoreService>();
        services.AddScoped<CatalogReferenceService>();
        services.AddScoped<
            IQueryHandler<SearchProductsQuery, Result<PagedResult<ProductResponse>>>,
            SearchProductsQueryHandler>();
        services.AddScoped<
            IQueryHandler<GetProductQuery, Result<ProductResponse>>,
            GetProductQueryHandler>();
        services.AddScoped<
            ICommandHandler<CreateProductCommand, Result<ProductResponse>>,
            CreateProductCommandHandler>();
        services.AddScoped<
            ICommandHandler<UpdateProductCommand, Result<ProductResponse>>,
            UpdateProductCommandHandler>();
        services.AddScoped<
            ICommandHandler<ManageProductImageCommand, Result<ProductImageResponse>>,
            ManageProductImageCommandHandler>();
        services.AddScoped<
            ICommandHandler<CreateStoreCommand, Result<StoreResponse>>,
            CreateStoreCommandHandler>();
        services.AddScoped<
            IQueryHandler<GetStoreByIdQuery, Result<StoreResponse>>,
            GetStoreByIdQueryHandler>();
        services.AddScoped<
            IQueryHandler<GetStoresByOwnerQuery, Result<IReadOnlyCollection<StoreResponse>>>,
            GetStoresByOwnerQueryHandler>();
        services.AddScoped<
            IQueryHandler<GetCatalogCategoriesQuery, IReadOnlyCollection<CatalogCategoryResponse>>,
            GetCatalogCategoriesQueryHandler>();
        services.AddScoped<
            IQueryHandler<GetCatalogBrandsQuery, IReadOnlyCollection<CatalogBrandResponse>>,
            GetCatalogBrandsQueryHandler>();
        services.AddScoped<
            IQueryHandler<GetCatalogMetricsQuery, CatalogMetricsResponse>,
            GetCatalogMetricsQueryHandler>();
        return services;
    }
}
