using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Identity.Application.Users;

namespace ECommerce.Identity.Application.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler
    : ICommandHandler<RegisterUserCommand, Result<UserResponse>>
{
    private readonly UserRegistrationService userService;

    public RegisterUserCommandHandler(UserRegistrationService userService)
    {
        this.userService = userService;
    }

    public Task<Result<UserResponse>> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        return userService.RegisterAsync(
            command.Email,
            command.DisplayName,
            command.Password,
            command.Role,
            cancellationToken);
    }
}
