using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Products;

namespace ECommerce.Catalog.Application.Commands.CreateProduct;

public sealed record CreateProductCommand(
    CreateProductRequest Request,
    ProductAccessContext Access,
    string Culture) : ICommand<Result<ProductResponse>>;
