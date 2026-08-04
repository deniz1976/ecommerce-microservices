using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.References;

namespace ECommerce.Catalog.Application.Commands.UpdateCatalogBrand;

public sealed record UpdateCatalogBrandCommand(
    Guid BrandId,
    UpdateCatalogBrandRequest Request)
    : ICommand<Result<ManagedCatalogBrandResponse>>;
