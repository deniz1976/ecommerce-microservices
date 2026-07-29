using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace ECommerce.ContractTests.Support;

internal sealed class RecordingAuthenticationService : IAuthenticationService
{
    public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
    {
        return Task.FromResult(AuthenticateResult.NoResult());
    }

    public Task ChallengeAsync(
        HttpContext context,
        string? scheme,
        AuthenticationProperties? properties)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.Headers.WWWAuthenticate = "Bearer";
        return Task.CompletedTask;
    }

    public Task ForbidAsync(
        HttpContext context,
        string? scheme,
        AuthenticationProperties? properties)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    }

    public Task SignInAsync(
        HttpContext context,
        string? scheme,
        ClaimsPrincipal principal,
        AuthenticationProperties? properties)
    {
        throw new NotSupportedException();
    }

    public Task SignOutAsync(
        HttpContext context,
        string? scheme,
        AuthenticationProperties? properties)
    {
        throw new NotSupportedException();
    }
}
