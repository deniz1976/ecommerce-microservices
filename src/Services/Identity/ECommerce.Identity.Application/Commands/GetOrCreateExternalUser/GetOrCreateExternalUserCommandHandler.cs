using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Identity.Application.Users;

namespace ECommerce.Identity.Application.Commands.GetOrCreateExternalUser;

public sealed class GetOrCreateExternalUserCommandHandler
    : ICommandHandler<GetOrCreateExternalUserCommand, Result<UserResponse>>
{
    private readonly UserService userService;

    public GetOrCreateExternalUserCommandHandler(UserService userService)
    {
        this.userService = userService;
    }

    public Task<Result<UserResponse>> HandleAsync(
        GetOrCreateExternalUserCommand command,
        CancellationToken cancellationToken)
    {
        return userService.GetOrCreateExternalAsync(
            command.Profile,
            cancellationToken);
    }
}
