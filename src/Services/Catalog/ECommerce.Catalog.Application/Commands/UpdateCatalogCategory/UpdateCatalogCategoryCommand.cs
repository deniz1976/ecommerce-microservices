using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.References;

namespace ECommerce.Catalog.Application.Commands.UpdateCatalogCategory;

public sealed record UpdateCatalogCategoryCommand(
    Guid CategoryId,
    UpdateCatalogCategoryRequest Request)
    : ICommand<Result<ManagedCatalogCategoryResponse>>;
