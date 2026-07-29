using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace ECommerce.ContractTests.Support;

internal sealed class HeaderAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationScheme = "HostedIntegration";
    public const string Token = "hosted-integration-token";

    public HeaderAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? token = ReadToken();
        if (!string.Equals(token, Token, StringComparison.Ordinal))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, "hosted-integration-user")
        ];
        ClaimsIdentity identity = new(claims, AuthenticationScheme);
        AuthenticationTicket ticket = new(
            new ClaimsPrincipal(identity),
            AuthenticationScheme);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.Headers.WWWAuthenticate = "Bearer";
        return Task.CompletedTask;
    }

    private string? ReadToken()
    {
        string authorization = Request.Headers.Authorization.ToString();
        const string bearerPrefix = "Bearer ";
        if (authorization.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return authorization[bearerPrefix.Length..];
        }

        return Request.Query.TryGetValue("access_token", out StringValues queryToken)
            ? queryToken.ToString()
            : null;
    }
}
