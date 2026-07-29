using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Products;

namespace ECommerce.Catalog.Application.Queries.GetProduct;

public sealed class GetProductQueryHandler
    : IQueryHandler<GetProductQuery, Result<ProductResponse>>
{
    private readonly ProductService productService;

    public GetProductQueryHandler(ProductService productService)
    {
        this.productService = productService;
    }

    public Task<Result<ProductResponse>> HandleAsync(
        GetProductQuery query,
        CancellationToken cancellationToken)
    {
        return query.Managed
            ? productService.GetManagedByIdAsync(
                query.Id,
                query.Access ?? new ProductAccessContext(null, false),
                query.Culture,
                cancellationToken)
            : productService.GetPublicByIdAsync(
                query.Id,
                query.Culture,
                cancellationToken);
    }
}
