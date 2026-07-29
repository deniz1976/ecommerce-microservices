using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Identity.Api.Errors;
using ECommerce.Identity.Application.AdminUsers;
using ECommerce.Identity.Application.Commands.RegisterUser;
using ECommerce.Identity.Application.Queries.GetUserById;
using ECommerce.Identity.Application.Queries.SearchAdminUsers;
using ECommerce.Identity.Application.Users;
using ECommerce.Identity.Domain;

namespace ECommerce.Identity.Api.Users;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/v1/users")
            .WithTags("Users");

        group.MapPost("/", RegisterAsync)
            .WithName("RegisterUser")
            .AllowAnonymous();

        group.MapGet("/", SearchAsync)
            .RequireAuthorization(AuthorizationPolicies.Admin)
            .WithName("SearchUsers");

        group.MapGet("/{id:guid}", GetByIdAsync)
            .RequireAuthorization(AuthorizationPolicies.Admin)
            .WithName("GetUserById");

        return endpoints;
    }

    private static async Task<IResult> SearchAsync(
        IQueryHandler<SearchAdminUsersQuery, PagedResult<AdminUserResponse>> queryHandler,
        int pageNumber = 1,
        int pageSize = 20,
        string? search = null,
        string? role = null,
        UserStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        PagedResult<AdminUserResponse> result = await queryHandler.HandleAsync(
            new SearchAdminUsersQuery(pageNumber, pageSize, search, role, status),
            cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> RegisterAsync(
        RegisterUserRequest request,
        ICommandHandler<RegisterUserCommand, Result<UserResponse>> commandHandler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<UserResponse> result = await commandHandler.HandleAsync(
            new RegisterUserCommand(
                request.Email,
                request.DisplayName,
                request.Password,
                request.Role),
            cancellationToken);

        return result.IsFailure
            ? IdentityResults.FromResult(result, httpContext)
            : Results.Created($"/api/v1/users/{result.Value!.Id}", result.Value);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        IQueryHandler<GetUserByIdQuery, Result<UserResponse>> queryHandler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<UserResponse> result = await queryHandler.HandleAsync(
            new GetUserByIdQuery(id),
            cancellationToken);
        return IdentityResults.FromResult(result, httpContext);
    }
}
