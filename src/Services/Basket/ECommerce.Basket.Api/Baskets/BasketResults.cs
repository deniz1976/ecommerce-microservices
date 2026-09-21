using ECommerce.Basket.Application;
using ECommerce.BuildingBlocks.Contracts.Api;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Localization;

namespace ECommerce.Basket.Api.Baskets;

public static class BasketResults
{
    public static IResult Forbidden(HttpContext httpContext) =>
        FromResult(
            Result<object>.Failure(new Error(ErrorCodes.AccessDenied, ErrorCodes.AccessDenied)),
            httpContext);

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
            ErrorCodes.BasketNotFound => StatusCodes.Status404NotFound,
            ErrorCodes.ProductNotFound => StatusCodes.Status404NotFound,
            ErrorCodes.AccessDenied => StatusCodes.Status403Forbidden,
            BasketErrorCodes.EmptyBasket => StatusCodes.Status400BadRequest,
            BasketErrorCodes.InvalidBasketItem => StatusCodes.Status400BadRequest,
            BasketErrorCodes.CurrencyMismatch => StatusCodes.Status400BadRequest,
            BasketErrorCodes.InvalidCheckoutAddress => StatusCodes.Status400BadRequest,
            BasketErrorCodes.ProductCatalogUnavailable => StatusCodes.Status503ServiceUnavailable,
            BasketErrorCodes.BasketStoreUnavailable => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static string ResolveMessage(string code, string culture, IErrorMessageLocalizer localizer)
    {
        return code switch
        {
            BasketErrorCodes.EmptyBasket => culture == "tr" ? "Sepet boş." : "Basket is empty.",
            BasketErrorCodes.InvalidBasketItem => culture == "tr" ? "Sepet ürünü geçersiz." : "Basket item is invalid.",
            BasketErrorCodes.ProductCatalogUnavailable => culture == "tr" ? "Ürün kataloğuna şu anda ulaşılamıyor." : "The product catalog is currently unavailable.",
            BasketErrorCodes.BasketStoreUnavailable => culture == "tr" ? "Sepet deposuna şu anda ulaşılamıyor. Sepetiniz korunuyor, lütfen birazdan yeniden deneyin." : "The basket store is currently unavailable. Your basket is preserved, please try again shortly.",
            BasketErrorCodes.CurrencyMismatch => culture == "tr" ? "Sepette yalnızca aynı para birimindeki ürünler bulunabilir." : "A basket can contain products in only one currency.",
            BasketErrorCodes.InvalidCheckoutAddress => culture == "tr" ? "Teslimat adresi geçersiz." : "The shipping address is invalid.",
            _ => localizer.GetMessage(code, culture)
        };
    }
}
