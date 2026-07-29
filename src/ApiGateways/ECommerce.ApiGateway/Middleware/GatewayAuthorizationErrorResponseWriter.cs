using ECommerce.BuildingBlocks.Contracts.Api;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Localization;

namespace ECommerce.ApiGateway.Middleware;

public static class GatewayAuthorizationErrorResponseWriter
{
    public static async Task WriteIfNeededAsync(HttpContext context)
    {
        string? errorCode = context.Response.StatusCode switch
        {
            StatusCodes.Status401Unauthorized => ErrorCodes.AuthenticationRequired,
            StatusCodes.Status403Forbidden => ErrorCodes.AccessDenied,
            _ => null
        };

        if (errorCode is null ||
            context.Response.HasStarted ||
            context.Response.ContentLength is > 0 ||
            !string.IsNullOrWhiteSpace(context.Response.ContentType))
        {
            return;
        }

        if (context.Response.StatusCode == StatusCodes.Status401Unauthorized &&
            !context.Response.Headers.ContainsKey("WWW-Authenticate"))
        {
            context.Response.Headers.WWWAuthenticate = "Bearer";
        }

        string culture = context.Request.Headers.AcceptLanguage.ToString();
        string message = context.RequestServices
            .GetRequiredService<IErrorMessageLocalizer>()
            .GetMessage(errorCode, culture);
        ApiErrorResponse response = new(context.TraceIdentifier, errorCode, message);

        await context.Response.WriteAsJsonAsync(response, context.RequestAborted);
    }
}
