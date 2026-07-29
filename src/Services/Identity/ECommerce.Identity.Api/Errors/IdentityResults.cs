using ECommerce.BuildingBlocks.Contracts.Api;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Localization;
using ECommerce.Identity.Application;

namespace ECommerce.Identity.Api.Errors;

public static class IdentityResults
{
    public static IResult FromResult<T>(Result<T> result, HttpContext httpContext)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        string culture = RequestCultureReader.Read(httpContext);
        Error error = result.Error ?? new Error(ErrorCodes.UnexpectedError, ErrorCodes.UnexpectedError);
        string message = ResolveMessage(
            error.Code,
            culture,
            httpContext.RequestServices.GetRequiredService<IErrorMessageLocalizer>());
        ApiErrorResponse response = new(httpContext.TraceIdentifier, error.Code, message, error.Details);

        return Results.Json(response, statusCode: ResolveStatusCode(error.Code));
    }

    private static int ResolveStatusCode(string code)
    {
        return code switch
        {
            IdentityErrorCodes.UserAlreadyExists => StatusCodes.Status409Conflict,
            IdentityErrorCodes.UserNotFound => StatusCodes.Status404NotFound,
            IdentityErrorCodes.ExternalRoleSynchronizationFailed => StatusCodes.Status503ServiceUnavailable,
            ErrorCodes.ValidationFailed => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static string ResolveMessage(string code, string culture, IErrorMessageLocalizer localizer)
    {
        return code switch
        {
            IdentityErrorCodes.UserAlreadyExists => culture == SupportedCultures.Turkish
                ? "Bu e-posta adresiyle kayıtlı bir kullanıcı zaten var."
                : "A user with this email address already exists.",
            IdentityErrorCodes.UserNotFound => culture == SupportedCultures.Turkish
                ? "Kullanıcı bulunamadı."
                : "User was not found.",
            IdentityErrorCodes.ExternalRoleSynchronizationFailed => culture == SupportedCultures.Turkish
                ? "Kullanıcı rolü kimlik sağlayıcısıyla eşitlenemedi."
                : "The user role could not be synchronized with the identity provider.",
            _ => localizer.GetMessage(code, culture)
        };
    }
}
