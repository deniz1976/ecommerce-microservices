using ECommerce.BuildingBlocks.Contracts.Api;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Localization;

namespace ECommerce.Payment.Api.Payments;

public static class PaymentResults
{
    public static IResult FromResult<T>(Result<T> result, HttpContext httpContext)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        string culture = RequestCultureReader.Read(httpContext);
        Error error = result.Error ?? new Error(ErrorCodes.UnexpectedError, ErrorCodes.UnexpectedError);
        string message = httpContext.RequestServices
            .GetRequiredService<IErrorMessageLocalizer>()
            .GetMessage(error.Code, culture);
        ApiErrorResponse response = new(httpContext.TraceIdentifier, error.Code, message, error.Details);

        int statusCode = error.Code == ErrorCodes.PaymentNotFound
            ? StatusCodes.Status404NotFound
            : StatusCodes.Status500InternalServerError;
        return Results.Json(response, statusCode: statusCode);
    }
}
