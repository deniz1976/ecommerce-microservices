using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.References;

namespace ECommerce.Catalog.Application.Queries.GetManagedCatalogCategories;

public sealed record GetManagedCatalogCategoriesQuery
    (ManagedCatalogCategoryListCriteria Criteria)
    : IQuery<PagedResult<ManagedCatalogCategoryResponse>>;
