using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Products;

namespace ECommerce.Catalog.Application.Commands.ManageProductImage;

public sealed class DeleteProductImageCommandHandler(ProductImageDeletionService service)
    : ICommandHandler<DeleteProductImageCommand, Result<ProductImageResponse>>
{
    public Task<Result<ProductImageResponse>> HandleAsync(DeleteProductImageCommand command, CancellationToken cancellationToken) =>
        service.DeleteAsync(command.ProductId, command.ImageId, command.Access, cancellationToken);
}
