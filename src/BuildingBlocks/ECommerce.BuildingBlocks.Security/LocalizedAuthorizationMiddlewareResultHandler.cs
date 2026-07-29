using ECommerce.BuildingBlocks.Contracts.Api;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Localization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.BuildingBlocks.Security;

public sealed class LocalizedAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Succeeded)
        {
            await next(context);
            return;
        }

        string errorCode;

        if (authorizeResult.Challenged)
        {
            await context.ChallengeAsync();
            errorCode = ErrorCodes.AuthenticationRequired;
        }
        else if (authorizeResult.Forbidden)
        {
            await context.ForbidAsync();
            errorCode = ErrorCodes.AccessDenied;
        }
        else
        {
            throw new InvalidOperationException("Authorization produced an unsupported result.");
        }

        if (context.Response.HasStarted)
        {
            return;
        }

        string culture = context.Request.Headers.AcceptLanguage.ToString();
        string message = context.RequestServices
            .GetRequiredService<IErrorMessageLocalizer>()
            .GetMessage(errorCode, culture);
        ApiErrorResponse response = new(context.TraceIdentifier, errorCode, message);

        await context.Response.WriteAsJsonAsync(response, context.RequestAborted);
    }
}
