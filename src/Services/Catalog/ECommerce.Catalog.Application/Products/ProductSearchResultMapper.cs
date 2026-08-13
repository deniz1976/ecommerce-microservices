using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.Products;

internal static class ProductSearchResultMapper
{
    public static Result<PagedResult<ProductResponse>> ToResult(
        PagedResult<Product> products,
        string culture) =>
        Result<PagedResult<ProductResponse>>.Success(new PagedResult<ProductResponse>(
            products.Items.Select(product => product.ToResponse(culture)).ToArray(),
            products.PageNumber,
            products.PageSize,
            products.TotalCount));
}
