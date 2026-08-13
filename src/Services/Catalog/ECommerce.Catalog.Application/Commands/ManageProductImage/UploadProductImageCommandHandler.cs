using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Products;

namespace ECommerce.Catalog.Application.Commands.ManageProductImage;

public sealed class UploadProductImageCommandHandler(ProductImageUploadService service)
    : ICommandHandler<UploadProductImageCommand, Result<ProductImageResponse>>
{
    public Task<Result<ProductImageResponse>> HandleAsync(UploadProductImageCommand command, CancellationToken cancellationToken) =>
        service.UploadAsync(command.ProductId, command.Upload, command.Access, cancellationToken);
}
