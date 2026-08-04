using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.References;

namespace ECommerce.Catalog.Application.Commands.CreateCatalogBrand;

public sealed record CreateCatalogBrandCommand(CreateCatalogBrandRequest Request)
    : ICommand<Result<CatalogBrandResponse>>;
