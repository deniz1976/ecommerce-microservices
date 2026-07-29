using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Products;

namespace ECommerce.Catalog.Application.Commands.ManageProductImage;

public sealed class ManageProductImageCommandHandler
    : ICommandHandler<ManageProductImageCommand, Result<ProductImageResponse>>
{
    private readonly ProductImageService imageService;

    public ManageProductImageCommandHandler(ProductImageService imageService)
    {
        this.imageService = imageService;
    }

    public Task<Result<ProductImageResponse>> HandleAsync(
        ManageProductImageCommand command,
        CancellationToken cancellationToken)
    {
        return command.Operation switch
        {
            ProductImageOperation.Upload when command.Upload is not null =>
                imageService.UploadAsync(
                    command.ProductId,
                    command.Upload,
                    command.Access,
                    cancellationToken),
            ProductImageOperation.SetMain when command.ImageId is not null =>
                imageService.SetMainAsync(
                    command.ProductId,
                    command.ImageId.Value,
                    command.Access,
                    cancellationToken),
            ProductImageOperation.Delete when command.ImageId is not null =>
                imageService.DeleteAsync(
                    command.ProductId,
                    command.ImageId.Value,
                    command.Access,
                    cancellationToken),
            _ => throw new ArgumentException(
                "The product image command is incomplete.",
                nameof(command))
        };
    }
}
