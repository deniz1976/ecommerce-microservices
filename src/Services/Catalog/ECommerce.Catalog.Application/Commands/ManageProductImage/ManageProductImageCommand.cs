using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Images;
using ECommerce.Catalog.Application.Products;

namespace ECommerce.Catalog.Application.Commands.ManageProductImage;

public sealed record ManageProductImageCommand(
    Guid ProductId,
    ProductImageOperation Operation,
    ProductAccessContext Access,
    Guid? ImageId = null,
    ProductImageUpload? Upload = null) : ICommand<Result<ProductImageResponse>>;
