using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Products;

namespace ECommerce.Catalog.Application.Queries.SearchProducts;

public sealed record SearchProductsQuery(
    ProductListQuery Filter,
    string Culture)
    : IQuery<Result<PagedResult<ProductResponse>>>;
