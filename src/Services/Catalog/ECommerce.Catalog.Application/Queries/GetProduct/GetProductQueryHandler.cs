using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Products;

namespace ECommerce.Catalog.Application.Queries.GetProduct;

public sealed class GetProductQueryHandler
    : IQueryHandler<GetProductQuery, Result<ProductResponse>>
{
    private readonly PublicProductQueryService productService;

    public GetProductQueryHandler(PublicProductQueryService productService)
    {
        this.productService = productService;
    }

    public Task<Result<ProductResponse>> HandleAsync(
        GetProductQuery query,
        CancellationToken cancellationToken)
    {
        return productService.GetByIdAsync(query.Id, query.Culture, cancellationToken);
    }
}
