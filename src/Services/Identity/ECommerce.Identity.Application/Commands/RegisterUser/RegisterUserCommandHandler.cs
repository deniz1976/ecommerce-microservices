using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Identity.Application.Users;
using ECommerce.Identity.Domain;

namespace ECommerce.Identity.Application.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler
    : ICommandHandler<RegisterUserCommand, Result<UserResponse>>
{
    private readonly IRepository<Domain.User, Guid> repository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IUserIdentityReader userIdentityReader;
    private readonly IPasswordHashService passwordHashService;

    public RegisterUserCommandHandler(
        IRepository<Domain.User, Guid> repository,
        IUnitOfWork unitOfWork,
        IUserIdentityReader userIdentityReader,
        IPasswordHashService passwordHashService)
    {
        this.repository = repository;
        this.unitOfWork = unitOfWork;
        this.userIdentityReader = userIdentityReader;
        this.passwordHashService = passwordHashService;
    }

    public async Task<Result<UserResponse>> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        string email = command.Email.Trim().ToLowerInvariant();
        string displayName = command.DisplayName.Trim();
        string requestedRole = string.IsNullOrWhiteSpace(command.Role)
            ? UserRoleNames.Customer
            : command.Role.Trim();

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(displayName) ||
            command.Password.Length < 8 ||
            !UserRoleNames.IsSelfServiceRole(requestedRole))
        {
            return Result<UserResponse>.Failure(
                new Error(ErrorCodes.ValidationFailed, ErrorCodes.ValidationFailed));
        }

        if (await userIdentityReader.FindIdByEmailAsync(email, cancellationToken) is not null)
        {
            return Result<UserResponse>.Failure(
                new Error(IdentityErrorCodes.UserAlreadyExists, IdentityErrorCodes.UserAlreadyExists));
        }

        string role = UserRoleNames.NormalizeSelfServiceRole(requestedRole);
        Domain.User user = new(Guid.NewGuid(), email, displayName, string.Empty);
        string passwordHash = passwordHashService.HashPassword(user, command.Password);
        user = new Domain.User(user.Id, email, displayName, passwordHash);
        user.SetSelfServiceRole(role);
        user.CompleteOnboarding();

        repository.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UserResponse>.Success(user.ToResponse());
    }
}
