using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Images;
using ECommerce.Catalog.Application.Products;

namespace ECommerce.Catalog.Application.Commands.ManageProductImage;

public sealed record UploadProductImageCommand(
    Guid ProductId, ProductImageUpload Upload, ProductAccessContext Access)
    : ICommand<Result<ProductImageResponse>>;
