using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Products;

namespace ECommerce.Catalog.Application.Queries.SearchProducts;

public sealed class SearchProductsQueryHandler
    : IQueryHandler<SearchProductsQuery, Result<PagedResult<ProductResponse>>>
{
    private readonly PublicProductQueryService productService;

    public SearchProductsQueryHandler(PublicProductQueryService productService)
    {
        this.productService = productService;
    }

    public Task<Result<PagedResult<ProductResponse>>> HandleAsync(
        SearchProductsQuery query,
        CancellationToken cancellationToken)
    {
        return productService.SearchAsync(query.Filter, query.Culture, cancellationToken);
    }
}
