using ECommerce.BuildingBlocks.Contracts.Api;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Localization;

namespace ECommerce.Notification.Api.Notifications;

public static class NotificationResults
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

        Error error = result.Error ??
            new Error(ErrorCodes.UnexpectedError, ErrorCodes.UnexpectedError);
        string culture = RequestCultureReader.Read(httpContext);
        string message = httpContext.RequestServices
            .GetRequiredService<IErrorMessageLocalizer>()
            .GetMessage(error.Code, culture);
        ApiErrorResponse response = new(
            httpContext.TraceIdentifier,
            error.Code,
            message,
            error.Details);

        int statusCode = error.Code switch
        {
            ErrorCodes.NotificationNotFound => StatusCodes.Status404NotFound,
            ErrorCodes.AccessDenied => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };
        return Results.Json(response, statusCode: statusCode);
    }
}
