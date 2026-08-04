using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.References;

namespace ECommerce.Catalog.Application.Commands.CreateCatalogCategory;

public sealed record CreateCatalogCategoryCommand(
    CreateCatalogCategoryRequest Request,
    string Culture)
    : ICommand<Result<CatalogCategoryResponse>>;
