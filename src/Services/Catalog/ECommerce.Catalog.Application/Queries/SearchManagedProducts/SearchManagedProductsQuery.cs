using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Products;

namespace ECommerce.Catalog.Application.Queries.SearchManagedProducts;

public sealed record SearchManagedProductsQuery(
    ProductListQuery Filter,
    ProductAccessContext Access,
    string Culture) : IQuery<Result<PagedResult<ProductResponse>>>;
