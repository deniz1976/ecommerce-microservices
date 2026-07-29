using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Identity.Application.Users;

namespace ECommerce.Identity.Application.Commands.SelectExternalUserRole;

public sealed record SelectExternalUserRoleCommand(
    ExternalUserProfile Profile,
    string Role) : ICommand<Result<UserResponse>>;
