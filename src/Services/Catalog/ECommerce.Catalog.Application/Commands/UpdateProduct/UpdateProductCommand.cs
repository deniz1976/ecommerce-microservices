using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Products;

namespace ECommerce.Catalog.Application.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    UpdateProductRequest Request,
    ProductAccessContext Access,
    string Culture) : ICommand<Result<ProductResponse>>;
