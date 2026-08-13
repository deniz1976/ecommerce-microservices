using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Products;

namespace ECommerce.Catalog.Application.Queries.SearchManagedProducts;

public sealed class SearchManagedProductsQueryHandler(ManagedProductQueryService service)
    : IQueryHandler<SearchManagedProductsQuery, Result<PagedResult<ProductResponse>>>
{
    public Task<Result<PagedResult<ProductResponse>>> HandleAsync(
        SearchManagedProductsQuery query,
        CancellationToken cancellationToken) =>
        service.SearchAsync(query.Filter, query.Access, query.Culture, cancellationToken);
}
