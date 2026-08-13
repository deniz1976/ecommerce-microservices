using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Identity.Application.Users;

public sealed class ExternalUserProvisioningService : IExternalUserProvisioningService
{
    private readonly IRepository<Domain.User, Guid> repository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IUserIdentityReader userIdentityReader;

    public ExternalUserProvisioningService(
        IRepository<Domain.User, Guid> repository,
        IUnitOfWork unitOfWork,
        IUserIdentityReader userIdentityReader)
    {
        this.repository = repository;
        this.unitOfWork = unitOfWork;
        this.userIdentityReader = userIdentityReader;
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

    private static Result<UserResponse> ValidationFailure() =>
        Result<UserResponse>.Failure(
            new Error(ErrorCodes.ValidationFailed, ErrorCodes.ValidationFailed));
}
