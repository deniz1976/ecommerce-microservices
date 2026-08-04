using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Identity.Domain;

namespace ECommerce.Identity.Application.Users;

public sealed class UserService : IExternalUserProvisioningService
{
    private readonly IRepository<Domain.User, Guid> repository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IUserIdentityReader userIdentityReader;
    private readonly IPasswordHashService passwordHashService;

    public UserService(
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

    public async Task<Result<UserResponse>> GetOrCreateExternalAsync(
        ExternalUserProfile profile,
        CancellationToken cancellationToken)
    {
        string provider = profile.Provider.Trim();
        string subject = profile.Subject.Trim();
        string email = profile.Email.Trim().ToLowerInvariant();
        string displayName = string.IsNullOrWhiteSpace(profile.DisplayName)
            ? email
            : profile.DisplayName.Trim();

        if (string.IsNullOrWhiteSpace(provider) ||
            string.IsNullOrWhiteSpace(subject) ||
            string.IsNullOrWhiteSpace(email))
        {
            return ValidationFailure();
        }

        Guid? externalUserId = await userIdentityReader.FindIdByExternalIdentityAsync(
            provider,
            subject,
            cancellationToken);
        if (externalUserId is not null)
        {
            Domain.User externalUser = (await repository.GetByIdAsync(
                externalUserId.Value,
                cancellationToken))!;
            return Result<UserResponse>.Success(externalUser.ToResponse());
        }

        Guid? existingUserId = await userIdentityReader.FindIdByEmailAsync(
            email,
            cancellationToken);
        if (existingUserId is not null)
        {
            Domain.User existingUser = (await repository.GetByIdAsync(
                existingUserId.Value,
                cancellationToken))!;
            existingUser.LinkExternalIdentity(provider, subject);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<UserResponse>.Success(existingUser.ToResponse());
        }

        Domain.User user = new(
            Guid.NewGuid(),
            email,
            displayName,
            string.Empty,
            provider,
            subject);
        repository.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<UserResponse>.Success(user.ToResponse());
    }

    public async Task<Result<UserResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        Domain.User? user = await repository.GetByIdAsync(id, cancellationToken);
        return user is null
            ? Result<UserResponse>.Failure(
                new Error(
                    IdentityErrorCodes.UserNotFound,
                    IdentityErrorCodes.UserNotFound))
            : Result<UserResponse>.Success(user.ToResponse());
    }

    private static Result<UserResponse> ValidationFailure()
    {
        return Result<UserResponse>.Failure(
            new Error(ErrorCodes.ValidationFailed, ErrorCodes.ValidationFailed));
    }
}
