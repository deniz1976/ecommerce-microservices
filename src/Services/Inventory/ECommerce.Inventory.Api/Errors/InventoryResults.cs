using ECommerce.BuildingBlocks.Contracts.Api;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Localization;

namespace ECommerce.Inventory.Api.Errors;

public static class InventoryResults
{
    public static IResult ProductNotFound(HttpContext httpContext)
    {
        return Create(
            httpContext,
            ErrorCodes.ProductNotFound,
            StatusCodes.Status404NotFound);
    }

    public static IResult InvalidQuantity(HttpContext httpContext)
    {
        string culture = RequestCultureReader.Read(httpContext);
        string fieldMessage = culture == SupportedCultures.Turkish
            ? "Stok miktarı sıfır veya daha büyük olmalıdır."
            : "Quantity on hand must be zero or greater.";
        Dictionary<string, string[]> details = new()
        {
            ["quantityOnHand"] = [fieldMessage]
        };
        return Create(
            httpContext,
            ErrorCodes.ValidationFailed,
            StatusCodes.Status400BadRequest,
            details);
    }

    private static IResult Create(
        HttpContext httpContext,
        string code,
        int statusCode,
        IReadOnlyDictionary<string, string[]>? details = null)
    {
        string culture = RequestCultureReader.Read(httpContext);
        string message = httpContext.RequestServices
            .GetRequiredService<IErrorMessageLocalizer>()
            .GetMessage(code, culture);
        ApiErrorResponse response = new(httpContext.TraceIdentifier, code, message, details);

        return Results.Json(response, statusCode: statusCode);
    }
}
