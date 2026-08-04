using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.References;

namespace ECommerce.Catalog.Application.Queries.GetManagedCatalogBrands;

public sealed record GetManagedCatalogBrandsQuery
    (ManagedCatalogBrandListCriteria Criteria)
    : IQuery<PagedResult<ManagedCatalogBrandResponse>>;
