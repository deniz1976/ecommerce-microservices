using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Catalog.Application.Commands.CreateProduct;
using ECommerce.Catalog.Application.Commands.UpdateProduct;
using ECommerce.Catalog.Application.Products;
using ECommerce.Catalog.Application.Queries.GetProduct;
using ECommerce.Catalog.Application.Queries.GetManagedProduct;
using ECommerce.Catalog.Application.Queries.SearchProducts;
using ECommerce.Catalog.Application.Queries.SearchManagedProducts;
using ECommerce.Catalog.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Catalog.Api.Products;

[ApiController]
[Route("api/v1/products")]
public sealed class ProductsController(
    ISender sender,
    IAuthenticatedUserResolver userResolver) : ControllerBase
{
    [HttpGet(Name = "SearchProducts")]
    [AllowAnonymous]
    public async Task<IResult> SearchPublicAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? brandId = null,
        [FromQuery] Guid? storeId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        string culture = RequestCultureReader.Read(HttpContext);
        ProductListQuery query = new(
            pageNumber,
            pageSize,
            search,
            categoryId,
            brandId,
            storeId,
            ProductStatus.Active,
            sortBy,
            sortDescending);
        Result<PagedResult<ProductResponse>> result = await sender.Send(
            new SearchProductsQuery(query, culture),
            cancellationToken);
        return CatalogResults.FromResult(result, HttpContext);
    }

    [HttpGet("{id:guid}", Name = "GetProductById")]
    [AllowAnonymous]
    public async Task<IResult> GetPublicByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        string culture = RequestCultureReader.Read(HttpContext);
        Result<ProductResponse> result = await sender.Send(
            new GetProductQuery(id, culture),
            cancellationToken);
        return CatalogResults.FromResult(result, HttpContext);
    }

    [HttpGet("manage", Name = "SearchManagedProducts")]
    [Authorize(Policy = AuthorizationPolicies.SellerOrAdmin)]
    public async Task<IResult> SearchManagedAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? brandId = null,
        [FromQuery] Guid? storeId = null,
        [FromQuery] ProductStatus? status = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        string culture = RequestCultureReader.Read(HttpContext);
        ProductAccessContext access = await ResolveAccessAsync(cancellationToken);
        ProductListQuery query = new(
            pageNumber,
            pageSize,
            search,
            categoryId,
            brandId,
            storeId,
            status,
            sortBy,
            sortDescending);
        Result<PagedResult<ProductResponse>> result = await sender.Send(
            new SearchManagedProductsQuery(query, access, culture),
            cancellationToken);
        return CatalogResults.FromResult(result, HttpContext);
    }

    [HttpGet("manage/{id:guid}", Name = "GetManagedProductById")]
    [Authorize(Policy = AuthorizationPolicies.SellerOrAdmin)]
    public async Task<IResult> GetManagedByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        string culture = RequestCultureReader.Read(HttpContext);
        ProductAccessContext access = await ResolveAccessAsync(cancellationToken);
        Result<ProductResponse> result = await sender.Send(
            new GetManagedProductQuery(id, access, culture),
            cancellationToken);
        return CatalogResults.FromResult(result, HttpContext);
    }

    [HttpPost(Name = "CreateProduct")]
    [Authorize(Policy = AuthorizationPolicies.SellerOrAdmin)]
    public async Task<IResult> CreateAsync(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        string culture = RequestCultureReader.Read(HttpContext);
        ProductAccessContext access = await ResolveAccessAsync(cancellationToken);
        Result<ProductResponse> result = await sender.Send(
            new CreateProductCommand(request, access, culture),
            cancellationToken);
        return result.IsFailure
            ? CatalogResults.FromResult(result, HttpContext)
            : Results.Created($"/api/v1/products/{result.Value!.Id}", result.Value);
    }

    [HttpPut("{id:guid}", Name = "UpdateProduct")]
    [Authorize(Policy = AuthorizationPolicies.SellerOrAdmin)]
    public async Task<IResult> UpdateAsync(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        string culture = RequestCultureReader.Read(HttpContext);
        ProductAccessContext access = await ResolveAccessAsync(cancellationToken);
        Result<ProductResponse> result = await sender.Send(
            new UpdateProductCommand(id, request, access, culture),
            cancellationToken);
        return CatalogResults.FromResult(result, HttpContext);
    }

    private async Task<ProductAccessContext> ResolveAccessAsync(
        CancellationToken cancellationToken)
    {
        bool isAdmin = User.IsInRole(ApplicationRoles.Admin);
        Guid? userId = isAdmin
            ? null
            : await userResolver.ResolveUserIdAsync(cancellationToken);
        return new ProductAccessContext(userId, isAdmin);
    }
}
