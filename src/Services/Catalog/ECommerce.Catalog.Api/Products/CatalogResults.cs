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
            _ => localizer.GetMessage(code, culture)
        };
    }
}
