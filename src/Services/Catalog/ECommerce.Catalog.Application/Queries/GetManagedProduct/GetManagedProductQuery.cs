using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Products;

namespace ECommerce.Catalog.Application.Queries.GetManagedProduct;

public sealed record GetManagedProductQuery(
    Guid Id,
    ProductAccessContext Access,
    string Culture) : IQuery<Result<ProductResponse>>;
