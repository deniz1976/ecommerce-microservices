using ECommerce.BuildingBlocks.Contracts.Api;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Localization;

namespace ECommerce.Ordering.Api.Orders;

public static class OrderResults
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
        string message = httpContext.RequestServices.GetRequiredService<IErrorMessageLocalizer>().GetMessage(error.Code, culture);
        ApiErrorResponse response = new(httpContext.TraceIdentifier, error.Code, message, error.Details);

        return Results.Json(response, statusCode: ResolveStatusCode(error.Code));
    }

    private static int ResolveStatusCode(string code)
    {
        return code switch
        {
            ErrorCodes.OrderNotFound => StatusCodes.Status404NotFound,
            ErrorCodes.OrderNotCancellable => StatusCodes.Status409Conflict,
            ErrorCodes.AccessDenied => StatusCodes.Status403Forbidden,
            ErrorCodes.DependencyUnavailable => StatusCodes.Status503ServiceUnavailable,
            ErrorCodes.ValidationFailed => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}
