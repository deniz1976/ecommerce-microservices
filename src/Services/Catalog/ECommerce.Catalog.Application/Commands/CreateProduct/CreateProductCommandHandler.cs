using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Products;

namespace ECommerce.Catalog.Application.Commands.CreateProduct;

public sealed class CreateProductCommandHandler
    : ICommandHandler<CreateProductCommand, Result<ProductResponse>>
{
    private readonly ProductManagementService productService;

    public CreateProductCommandHandler(ProductManagementService productService)
    {
        this.productService = productService;
    }

    public Task<Result<ProductResponse>> HandleAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        return productService.CreateAsync(
            command.Request,
            command.Access,
            command.Culture,
            cancellationToken);
    }
}
