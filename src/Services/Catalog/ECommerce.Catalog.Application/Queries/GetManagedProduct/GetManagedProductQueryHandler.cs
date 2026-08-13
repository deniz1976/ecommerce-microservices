using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Products;

namespace ECommerce.Catalog.Application.Queries.GetManagedProduct;

public sealed class GetManagedProductQueryHandler(ManagedProductQueryService service)
    : IQueryHandler<GetManagedProductQuery, Result<ProductResponse>>
{
    public Task<Result<ProductResponse>> HandleAsync(
        GetManagedProductQuery query,
        CancellationToken cancellationToken) =>
        service.GetByIdAsync(query.Id, query.Access, query.Culture, cancellationToken);
}
