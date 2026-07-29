using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Products;

namespace ECommerce.Catalog.Application.Queries.SearchProducts;

public sealed record SearchProductsQuery(
    ProductListQuery Filter,
    string Culture,
    bool Managed,
    ProductAccessContext? Access = null)
    : IQuery<Result<PagedResult<ProductResponse>>>;
