using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Identity.Domain;

namespace ECommerce.Identity.Application.Users;

public sealed class UserService
{
    private readonly IUserRepository userRepository;
    private readonly IPasswordHashService passwordHashService;

    public UserService(IUserRepository userRepository, IPasswordHashService passwordHashService)
    {
        this.userRepository = userRepository;
        this.passwordHashService = passwordHashService;
    }

    public async Task<Result<UserResponse>> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        string email = request.Email.Trim().ToLowerInvariant();
        string displayName = request.DisplayName.Trim();
        string requestedRole = GetRequestedSelfServiceRole(request.Role);

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(displayName) ||
            request.Password.Length < 8 ||
            !UserRoleNames.IsSelfServiceRole(requestedRole))
        {
            return Result<UserResponse>.Failure(new Error(ErrorCodes.ValidationFailed, ErrorCodes.ValidationFailed));
        }

        string role = UserRoleNames.NormalizeSelfServiceRole(requestedRole);

        Domain.User? existing = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (existing is not null)
        {
            return Result<UserResponse>.Failure(new Error(IdentityErrorCodes.UserAlreadyExists, IdentityErrorCodes.UserAlreadyExists));
        }

        Domain.User user = new(Guid.NewGuid(), email, displayName, string.Empty);
        string passwordHash = passwordHashService.HashPassword(user, request.Password);
        user = new Domain.User(user.Id, email, displayName, passwordHash);
        user.SetSelfServiceRole(role);
        user.CompleteOnboarding();

        userRepository.Add(user);
        await userRepository.SaveChangesAsync(cancellationToken);

        return Result<UserResponse>.Success(user.ToResponse());
    }

    public async Task<Result<UserResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        Domain.User? user = await userRepository.GetByIdAsync(id, cancellationToken);

        return user is null
            ? Result<UserResponse>.Failure(new Error(IdentityErrorCodes.UserNotFound, IdentityErrorCodes.UserNotFound))
            : Result<UserResponse>.Success(user.ToResponse());
    }

    public async Task<Result<UserResponse>> GetOrCreateExternalUserAsync(ExternalUserProfile profile, CancellationToken cancellationToken)
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
            return Result<UserResponse>.Failure(new Error(ErrorCodes.ValidationFailed, ErrorCodes.ValidationFailed));
        }

        Domain.User? externalUser = await userRepository.GetByExternalIdentityAsync(provider, subject, cancellationToken);
        if (externalUser is not null)
        {
            return Result<UserResponse>.Success(externalUser.ToResponse());
        }

        Domain.User? existing = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (existing is not null)
        {
            existing.LinkExternalIdentity(provider, subject);
            await userRepository.SaveChangesAsync(cancellationToken);

            return Result<UserResponse>.Success(existing.ToResponse());
        }

        Domain.User user = new(Guid.NewGuid(), email, displayName, string.Empty, provider, subject);

        userRepository.Add(user);
        await userRepository.SaveChangesAsync(cancellationToken);

        return Result<UserResponse>.Success(user.ToResponse());
    }

    public async Task<Result<UserResponse>> SelectExternalUserRoleAsync(
        ExternalUserProfile profile,
        SelectUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        string requestedRole = GetRequestedSelfServiceRole(request.Role);
        if (!UserRoleNames.IsSelfServiceRole(requestedRole))
        {
            return Result<UserResponse>.Failure(new Error(ErrorCodes.ValidationFailed, ErrorCodes.ValidationFailed));
        }

        string role = UserRoleNames.NormalizeSelfServiceRole(requestedRole);

        Result<UserResponse> syncResult = await GetOrCreateExternalUserAsync(profile, cancellationToken);
        if (syncResult.IsFailure)
        {
            return syncResult;
        }

        string provider = profile.Provider.Trim();
        string subject = profile.Subject.Trim();
        Domain.User? user = await userRepository.ReplaceSelfServiceRoleAsync(
            provider,
            subject,
            role,
            cancellationToken);
        if (user is null)
        {
            return Result<UserResponse>.Failure(new Error(IdentityErrorCodes.UserNotFound, IdentityErrorCodes.UserNotFound));
        }

        return Result<UserResponse>.Success(user.ToResponse());
    }

    private static string GetRequestedSelfServiceRole(string? role)
    {
        return string.IsNullOrWhiteSpace(role)
            ? UserRoleNames.Customer
            : role.Trim();
    }
}
