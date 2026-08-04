using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Identity.Api.Errors;
using ECommerce.Identity.Application.AdminUsers;
using ECommerce.Identity.Application.Commands.RegisterUser;
using ECommerce.Identity.Application.Queries.GetUserById;
using ECommerce.Identity.Application.Queries.SearchAdminUsers;
using ECommerce.Identity.Application.Users;
using ECommerce.Identity.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Identity.Api.Users;

[ApiController]
[Route("api/v1/users")]
public sealed class UsersController(ISender sender) : ControllerBase
{
    [HttpPost(Name = "RegisterUser")]
    [AllowAnonymous]
    public async Task<IResult> RegisterAsync(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        Result<UserResponse> result = await sender.Send(
            new RegisterUserCommand(
                request.Email,
                request.DisplayName,
                request.Password,
                request.Role),
            cancellationToken);

        return result.IsFailure
            ? IdentityResults.FromResult(result, HttpContext)
            : Results.Created($"/api/v1/users/{result.Value!.Id}", result.Value);
    }

    [HttpGet(Name = "SearchUsers")]
    [Authorize(Policy = AuthorizationPolicies.Admin)]
    public async Task<PagedResult<AdminUserResponse>> SearchAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? role = null,
        [FromQuery] UserStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        return await sender.Send(
            new SearchAdminUsersQuery(pageNumber, pageSize, search, role, status),
            cancellationToken);
    }

    [HttpGet("{id:guid}", Name = "GetUserById")]
    [Authorize(Policy = AuthorizationPolicies.Admin)]
    public async Task<IResult> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        Result<AdminUserResponse> result = await sender.Send(
            new GetUserByIdQuery(id),
            cancellationToken);
        return IdentityResults.FromResult(result, HttpContext);
    }
}
