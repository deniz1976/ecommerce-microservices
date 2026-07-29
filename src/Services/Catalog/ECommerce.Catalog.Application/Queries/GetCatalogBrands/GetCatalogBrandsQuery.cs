using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Catalog.Application.References;

namespace ECommerce.Catalog.Application.Queries.GetCatalogBrands;

public sealed record GetCatalogBrandsQuery
    : IQuery<IReadOnlyCollection<CatalogBrandResponse>>;
