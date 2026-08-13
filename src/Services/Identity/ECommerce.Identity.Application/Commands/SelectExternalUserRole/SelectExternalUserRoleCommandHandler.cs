using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Identity.Application.Users;

namespace ECommerce.Identity.Application.Commands.SelectExternalUserRole;

public sealed class SelectExternalUserRoleCommandHandler(
    ExternalUserRoleSelectionService roleSelectionService)
    : ICommandHandler<SelectExternalUserRoleCommand, Result<UserResponse>>
{
    public Task<Result<UserResponse>> HandleAsync(
        SelectExternalUserRoleCommand command,
        CancellationToken cancellationToken) =>
        roleSelectionService.SelectAsync(command.Profile, command.Role, cancellationToken);
}
