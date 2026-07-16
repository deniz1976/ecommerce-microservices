using ECommerce.Basket.Application;
using ECommerce.BuildingBlocks.Contracts.Api;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Localization;

namespace ECommerce.Basket.Api.Baskets;

public static class BasketResults
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
            ErrorCodes.BasketNotFound => StatusCodes.Status404NotFound,
            BasketErrorCodes.EmptyBasket => StatusCodes.Status400BadRequest,
            BasketErrorCodes.InvalidBasketItem => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static string ResolveMessage(string code, string culture, IErrorMessageLocalizer localizer)
    {
        return code switch
        {
            BasketErrorCodes.EmptyBasket => culture == "tr" ? "Sepet boş." : "Basket is empty.",
            BasketErrorCodes.InvalidBasketItem => culture == "tr" ? "Sepet ürünü geçersiz." : "Basket item is invalid.",
            _ => localizer.GetMessage(code, culture)
        };
    }
}
