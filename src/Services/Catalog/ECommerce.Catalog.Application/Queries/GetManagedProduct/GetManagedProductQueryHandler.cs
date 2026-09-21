using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Products;

namespace ECommerce.Catalog.Application.Queries.GetManagedProduct;

public sealed class GetManagedProductQueryHandler(ManagedProductQueryService service)
    : IQueryHandler<GetManagedProductQuery, Result<ManagedProductResponse>>
{
    public Task<Result<ManagedProductResponse>> HandleAsync(
        GetManagedProductQuery query,
        CancellationToken cancellationToken) =>
        service.GetByIdAsync(query.Id, query.Access, query.Culture, cancellationToken);
}
