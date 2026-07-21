using ECommerce.BuildingBlocks.Contracts.Api;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Localization;
using ECommerce.Catalog.Application;

namespace ECommerce.Catalog.Api.Products;

public static class CatalogResults
{
    public static IResult FromResult<T>(Result<T> result, HttpContext httpContext)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        string culture = RequestCultureReader.Read(httpContext);
        Error error = result.Error ?? new Error(ErrorCodes.UnexpectedError, ErrorCodes.UnexpectedError);
        string message = ResolveMessage(error.Code, culture, httpContext.RequestServices.GetRequiredService<IErrorMessageLocalizer>());
        ApiErrorResponse response = new(httpContext.TraceIdentifier, error.Code, message, error.Details);

        return Results.Json(response, statusCode: ResolveStatusCode(error.Code));
    }

    private static int ResolveStatusCode(string code)
    {
        return code switch
        {
            ErrorCodes.ProductNotFound => StatusCodes.Status404NotFound,
            CatalogErrorCodes.CategoryNotFound => StatusCodes.Status400BadRequest,
            CatalogErrorCodes.BrandNotFound => StatusCodes.Status400BadRequest,
            CatalogErrorCodes.InvalidProductTranslation => StatusCodes.Status400BadRequest,
            CatalogErrorCodes.StoreNotFound => StatusCodes.Status404NotFound,
            CatalogErrorCodes.StoreAccessDenied => StatusCodes.Status403Forbidden,
            CatalogErrorCodes.IdentityResolutionFailed => StatusCodes.Status503ServiceUnavailable,
            CatalogErrorCodes.StoreSlugConflict => StatusCodes.Status409Conflict,
            CatalogErrorCodes.StoreRequired => StatusCodes.Status400BadRequest,
            ErrorCodes.ValidationFailed => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static string ResolveMessage(string code, string culture, IErrorMessageLocalizer localizer)
    {
        return code switch
        {
            CatalogErrorCodes.CategoryNotFound => culture == "tr" ? "Kategori bulunamadı." : "Category was not found.",
            CatalogErrorCodes.BrandNotFound => culture == "tr" ? "Marka bulunamadı." : "Brand was not found.",
            CatalogErrorCodes.InvalidProductTranslation => culture == "tr" ? "Ürün çevirileri geçersiz." : "Product translations are invalid.",
            CatalogErrorCodes.StoreNotFound => culture == "tr" ? "Mağaza bulunamadı." : "Store was not found.",
            CatalogErrorCodes.StoreRequired => culture == "tr" ? "Satıcı ürünleri için mağaza zorunludur." : "A store is required for seller products.",
            CatalogErrorCodes.StoreAccessDenied => culture == "tr" ? "Bu mağaza için yetkiniz yok." : "You do not have access to this store.",
            CatalogErrorCodes.StoreSlugConflict => culture == "tr" ? "Mağaza adresi zaten kullanılıyor." : "The store slug is already in use.",
            CatalogErrorCodes.IdentityResolutionFailed => culture == "tr" ? "Kullanıcı kimliği doğrulanamadı." : "The authenticated user could not be resolved.",
            _ => localizer.GetMessage(code, culture)
        };
    }
}
