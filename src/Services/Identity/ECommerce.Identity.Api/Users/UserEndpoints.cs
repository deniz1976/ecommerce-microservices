using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Identity.Application.Users;

namespace ECommerce.Identity.Api.Users;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/v1/users")
            .WithTags("Users");

        group.MapPost("/", RegisterAsync)
            .WithName("RegisterUser");

        group.MapGet("/{id:guid}", GetByIdAsync)
            .RequireAuthorization(AuthorizationPolicies.Admin)
            .WithName("GetUserById");

        return endpoints;
    }

    private static async Task<IResult> RegisterAsync(RegisterUserRequest request, UserService userService, CancellationToken cancellationToken)
    {
        Result<UserResponse> result = await userService.RegisterAsync(request, cancellationToken);

        return result.IsFailure
            ? Results.BadRequest(result.Error)
            : Results.Created($"/api/v1/users/{result.Value!.Id}", result.Value);
    }

    private static async Task<IResult> GetByIdAsync(Guid id, UserService userService, CancellationToken cancellationToken)
    {
        Result<UserResponse> result = await userService.GetByIdAsync(id, cancellationToken);

        return result.IsFailure
            ? Results.NotFound(result.Error)
            : Results.Ok(result.Value);
    }
}
