using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Catalog.Api.Products;
using ECommerce.Catalog.Application.Commands.CreateCatalogBrand;
using ECommerce.Catalog.Application.Commands.CreateCatalogCategory;
using ECommerce.Catalog.Application.Commands.UpdateCatalogBrand;
using ECommerce.Catalog.Application.Commands.UpdateCatalogCategory;
using ECommerce.Catalog.Application.Queries.GetCatalogBrands;
using ECommerce.Catalog.Application.Queries.GetCatalogCategories;
using ECommerce.Catalog.Application.Queries.GetManagedCatalogBrands;
using ECommerce.Catalog.Application.Queries.GetManagedCatalogCategories;
using ECommerce.Catalog.Application.References;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Catalog.Api.References;

[ApiController]
[Route("api/v1/catalog-references")]
public sealed class CatalogReferencesController(ISender sender) : ControllerBase
{
    [HttpGet("categories", Name = "GetCatalogCategories")]
    [AllowAnonymous]
    public async Task<IReadOnlyCollection<CatalogCategoryResponse>> GetCategoriesAsync(
        CancellationToken cancellationToken)
    {
        return await sender.Send(
            new GetCatalogCategoriesQuery(RequestCultureReader.Read(HttpContext)),
            cancellationToken);
    }

    [HttpGet("brands", Name = "GetCatalogBrands")]
    [AllowAnonymous]
    public async Task<IReadOnlyCollection<CatalogBrandResponse>> GetBrandsAsync(
        CancellationToken cancellationToken)
    {
        return await sender.Send(new GetCatalogBrandsQuery(), cancellationToken);
    }

    [HttpPost("categories", Name = "CreateCatalogCategory")]
    [Authorize(Policy = AuthorizationPolicies.Admin)]
    public async Task<IResult> CreateCategoryAsync(
        [FromBody] CreateCatalogCategoryRequest request,
        CancellationToken cancellationToken)
    {
        Result<CatalogCategoryResponse> result = await sender.Send(
            new CreateCatalogCategoryCommand(
                request,
                RequestCultureReader.Read(HttpContext)),
            cancellationToken);
        return result.IsFailure
            ? CatalogResults.FromResult(result, HttpContext)
            : Results.Created(
                $"/api/v1/catalog-references/categories/{result.Value!.Id}",
                result.Value);
    }

    [HttpPost("brands", Name = "CreateCatalogBrand")]
    [Authorize(Policy = AuthorizationPolicies.Admin)]
    public async Task<IResult> CreateBrandAsync(
        [FromBody] CreateCatalogBrandRequest request,
        CancellationToken cancellationToken)
    {
        Result<CatalogBrandResponse> result = await sender.Send(
            new CreateCatalogBrandCommand(request),
            cancellationToken);
        return result.IsFailure
            ? CatalogResults.FromResult(result, HttpContext)
            : Results.Created(
                $"/api/v1/catalog-references/brands/{result.Value!.Id}",
                result.Value);
    }

    [HttpGet("manage/categories", Name = "GetManagedCatalogCategories")]
    [Authorize(Policy = AuthorizationPolicies.Admin)]
    public async Task<PagedResult<ManagedCatalogCategoryResponse>>
        GetManagedCategoriesAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = false,
            CancellationToken cancellationToken = default)
    {
        return await sender.Send(
            new GetManagedCatalogCategoriesQuery(
                new ManagedCatalogCategoryListCriteria(
                    pageNumber,
                    pageSize,
                    search,
                    isActive,
                    sortBy,
                    sortDescending)),
            cancellationToken);
    }

    [HttpGet("manage/brands", Name = "GetManagedCatalogBrands")]
    [Authorize(Policy = AuthorizationPolicies.Admin)]
    public async Task<PagedResult<ManagedCatalogBrandResponse>>
        GetManagedBrandsAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = false,
            CancellationToken cancellationToken = default)
    {
        return await sender.Send(
            new GetManagedCatalogBrandsQuery(
                new ManagedCatalogBrandListCriteria(
                    pageNumber,
                    pageSize,
                    search,
                    isActive,
                    sortBy,
                    sortDescending)),
            cancellationToken);
    }

    [HttpPut("categories/{id:guid}", Name = "UpdateCatalogCategory")]
    [Authorize(Policy = AuthorizationPolicies.Admin)]
    public async Task<IResult> UpdateCategoryAsync(
        Guid id,
        [FromBody] UpdateCatalogCategoryRequest request,
        CancellationToken cancellationToken)
    {
        Result<ManagedCatalogCategoryResponse> result = await sender.Send(
            new UpdateCatalogCategoryCommand(id, request),
            cancellationToken);
        return CatalogResults.FromResult(result, HttpContext);
    }

    [HttpPut("brands/{id:guid}", Name = "UpdateCatalogBrand")]
    [Authorize(Policy = AuthorizationPolicies.Admin)]
    public async Task<IResult> UpdateBrandAsync(
        Guid id,
        [FromBody] UpdateCatalogBrandRequest request,
        CancellationToken cancellationToken)
    {
        Result<ManagedCatalogBrandResponse> result = await sender.Send(
            new UpdateCatalogBrandCommand(id, request),
            cancellationToken);
        return CatalogResults.FromResult(result, HttpContext);
    }
}
