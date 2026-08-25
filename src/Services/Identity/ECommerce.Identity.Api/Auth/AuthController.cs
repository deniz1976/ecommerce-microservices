using System.Security.Claims;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Identity.Api.Errors;
using ECommerce.Identity.Application.Commands.GetOrCreateExternalUser;
using ECommerce.Identity.Application.Commands.SelectExternalUserRole;
using ECommerce.Identity.Application.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Identity.Api.Auth;

[ApiController]
[Route("api/v1/auth")]
[Authorize(Policy = AuthorizationPolicies.AuthenticatedUser)]
public sealed class AuthController(ISender sender) : ControllerBase
{
    [HttpGet("me", Name = "GetCurrentUser")]
    public async Task<IResult> GetMeAsync(CancellationToken cancellationToken)
    {
        Result<UserResponse> result = await sender.Send(
            new GetOrCreateExternalUserCommand(CreateExternalUserProfile(User)),
            cancellationToken);
        return IdentityResults.FromResult(result, HttpContext);
    }

    [HttpPut("me/role", Name = "SelectCurrentUserRole")]
    public async Task<IResult> SelectRoleAsync(
        [FromBody] SelectUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        Result<UserResponse> result = await sender.Send(
            new SelectExternalUserRoleCommand(
                CreateExternalUserProfile(User),
                request.Role),
            cancellationToken);
        return IdentityResults.FromResult(result, HttpContext);
    }

    private static ExternalUserProfile CreateExternalUserProfile(ClaimsPrincipal principal)
    {
        string? subject = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");

        string? email = principal.FindFirstValue(ClaimTypes.Email)
            ?? principal.FindFirstValue("email")
            ?? principal.FindFirstValue(Auth0ClaimNames.Email);

        string? displayName = principal.FindFirstValue("name")
            ?? principal.FindFirstValue("nickname")
            ?? principal.FindFirstValue(Auth0ClaimNames.Name)
            ?? email;

        bool emailVerified = bool.TryParse(
            principal.FindFirstValue("email_verified")
                ?? principal.FindFirstValue(Auth0ClaimNames.EmailVerified),
            out bool verified) && verified;

        return new ExternalUserProfile(
            "Auth0",
            subject ?? string.Empty,
            email ?? string.Empty,
            emailVerified,
            displayName ?? string.Empty);
    }
}
