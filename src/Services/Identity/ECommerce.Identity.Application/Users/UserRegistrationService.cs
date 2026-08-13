using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Identity.Domain;

namespace ECommerce.Identity.Application.Users;

public sealed class UserRegistrationService
{
    private readonly IRepository<Domain.User, Guid> repository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IUserIdentityReader userIdentityReader;
    private readonly IPasswordHashService passwordHashService;

    public UserRegistrationService(
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

    public async Task<Result<UserResponse>> RegisterAsync(
        string emailValue,
        string displayNameValue,
        string password,
        string? roleValue,
        CancellationToken cancellationToken)
    {
        string email = emailValue.Trim().ToLowerInvariant();
        string displayName = displayNameValue.Trim();
        string requestedRole = string.IsNullOrWhiteSpace(roleValue)
            ? UserRoleNames.Customer
            : roleValue.Trim();

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(displayName) ||
            password.Length < 8 ||
            !UserRoleNames.IsSelfServiceRole(requestedRole))
        {
            return ValidationFailure();
        }

        if (await userIdentityReader.FindIdByEmailAsync(email, cancellationToken) is not null)
        {
            return Result<UserResponse>.Failure(
                new Error(
                    IdentityErrorCodes.UserAlreadyExists,
                    IdentityErrorCodes.UserAlreadyExists));
        }

        string role = UserRoleNames.NormalizeSelfServiceRole(requestedRole);
        Domain.User user = new(Guid.NewGuid(), email, displayName, string.Empty);
        string passwordHash = passwordHashService.HashPassword(user, password);
        user = new Domain.User(user.Id, email, displayName, passwordHash);
        user.SetSelfServiceRole(role);
        user.CompleteOnboarding();

        repository.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<UserResponse>.Success(user.ToResponse());
    }

    private static Result<UserResponse> ValidationFailure()
    {
        return Result<UserResponse>.Failure(
            new Error(ErrorCodes.ValidationFailed, ErrorCodes.ValidationFailed));
    }
}
