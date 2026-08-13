using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Products;

namespace ECommerce.Catalog.Application.Queries.SearchProducts;

public sealed class SearchProductsQueryHandler
    : IQueryHandler<SearchProductsQuery, Result<PagedResult<ProductResponse>>>
{
    private readonly ProductQueryService productService;

    public SearchProductsQueryHandler(ProductQueryService productService)
    {
        this.productService = productService;
    }

    public Task<Result<PagedResult<ProductResponse>>> HandleAsync(
        SearchProductsQuery query,
        CancellationToken cancellationToken)
    {
        return query.Managed
            ? productService.SearchManagedAsync(
                query.Filter,
                query.Access ?? new ProductAccessContext(null, false),
                query.Culture,
                cancellationToken)
            : productService.SearchPublicAsync(
                query.Filter,
                query.Culture,
                cancellationToken);
    }
}
