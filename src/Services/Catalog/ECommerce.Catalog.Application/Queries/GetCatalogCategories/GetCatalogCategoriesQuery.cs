using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Catalog.Application.References;

namespace ECommerce.Catalog.Application.Queries.GetCatalogCategories;

public sealed record GetCatalogCategoriesQuery(string Culture)
    : IQuery<IReadOnlyCollection<CatalogCategoryResponse>>;
