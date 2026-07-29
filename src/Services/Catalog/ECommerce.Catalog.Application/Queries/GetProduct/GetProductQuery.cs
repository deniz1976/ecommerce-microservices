using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Products;

namespace ECommerce.Catalog.Application.Queries.GetProduct;

public sealed record GetProductQuery(
    Guid Id,
    string Culture,
    bool Managed,
    ProductAccessContext? Access = null) : IQuery<Result<ProductResponse>>;
