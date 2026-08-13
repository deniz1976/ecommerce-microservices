using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Products;

namespace ECommerce.Catalog.Application.Commands.ManageProductImage;

public sealed record SetMainProductImageCommand(Guid ProductId, Guid ImageId, ProductAccessContext Access)
    : ICommand<Result<ProductImageResponse>>;
