using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Products;

namespace ECommerce.Catalog.Application.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler
    : ICommandHandler<UpdateProductCommand, Result<ProductResponse>>
{
    private readonly ProductManagementService productService;

    public UpdateProductCommandHandler(ProductManagementService productService)
    {
        this.productService = productService;
    }

    public Task<Result<ProductResponse>> HandleAsync(
        UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        return productService.UpdateAsync(
            command.Id,
            command.Request,
            command.Access,
            command.Culture,
            cancellationToken);
    }
}
