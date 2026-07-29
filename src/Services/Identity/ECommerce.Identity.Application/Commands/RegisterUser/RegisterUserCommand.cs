using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Identity.Application.Users;

namespace ECommerce.Identity.Application.Commands.RegisterUser;

public sealed record RegisterUserCommand(
    string Email,
    string DisplayName,
    string Password,
    string? Role) : ICommand<Result<UserResponse>>;
