using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Products;

namespace ECommerce.Catalog.Application.Commands.ManageProductImage;

public sealed class SetMainProductImageCommandHandler(ProductImageMainService service)
    : ICommandHandler<SetMainProductImageCommand, Result<ProductImageResponse>>
{
    public Task<Result<ProductImageResponse>> HandleAsync(SetMainProductImageCommand command, CancellationToken cancellationToken) =>
        service.SetMainAsync(command.ProductId, command.ImageId, command.Access, cancellationToken);
}
