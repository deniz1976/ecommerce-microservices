using System.Security.Claims;
using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Identity.Api.Errors;
using ECommerce.Identity.Application.Commands.GetOrCreateExternalUser;
using ECommerce.Identity.Application.Commands.SelectExternalUserRole;
using ECommerce.Identity.Application.Users;

namespace ECommerce.Identity.Api.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/v1/auth")
            .WithTags("Auth");

        group.MapGet("/me", GetMeAsync)
            .RequireAuthorization(AuthorizationPolicies.AuthenticatedUser)
            .WithName("GetCurrentUser");

        group.MapPut("/me/role", SelectRoleAsync)
            .RequireAuthorization(AuthorizationPolicies.AuthenticatedUser)
            .WithName("SelectCurrentUserRole");

        return endpoints;
    }

    private static async Task<IResult> GetMeAsync(
        ClaimsPrincipal principal,
        ICommandHandler<GetOrCreateExternalUserCommand, Result<UserResponse>> commandHandler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<UserResponse> result = await commandHandler.HandleAsync(
            new GetOrCreateExternalUserCommand(CreateExternalUserProfile(principal)),
            cancellationToken);

        return IdentityResults.FromResult(result, httpContext);
    }

    private static async Task<IResult> SelectRoleAsync(
        SelectUserRoleRequest request,
        ClaimsPrincipal principal,
        ICommandHandler<SelectExternalUserRoleCommand, Result<UserResponse>> commandHandler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<UserResponse> result = await commandHandler.HandleAsync(
            new SelectExternalUserRoleCommand(
                CreateExternalUserProfile(principal),
                request.Role),
            cancellationToken);

        return IdentityResults.FromResult(result, httpContext);
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

        return new ExternalUserProfile("Auth0", subject ?? string.Empty, email ?? string.Empty, displayName ?? string.Empty);
    }
}
