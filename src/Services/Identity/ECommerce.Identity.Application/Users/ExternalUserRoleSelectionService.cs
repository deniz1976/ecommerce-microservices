using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Identity.Domain;
using Microsoft.Extensions.Logging;

namespace ECommerce.Identity.Application.Users;

public sealed class ExternalUserRoleSelectionService
{
    private readonly IExternalUserProvisioningService userService;
    private readonly IExternalRoleSynchronizer externalRoleSynchronizer;
    private readonly ISelfServiceRoleWriter roleWriter;
    private readonly IRoleReconciliationQueue reconciliationQueue;
    private readonly ILogger<ExternalUserRoleSelectionService> logger;

    public ExternalUserRoleSelectionService(
        IExternalUserProvisioningService userService,
        IExternalRoleSynchronizer externalRoleSynchronizer,
        ISelfServiceRoleWriter roleWriter,
        IRoleReconciliationQueue reconciliationQueue,
        ILogger<ExternalUserRoleSelectionService> logger)
    {
        this.userService = userService;
        this.externalRoleSynchronizer = externalRoleSynchronizer;
        this.roleWriter = roleWriter;
        this.reconciliationQueue = reconciliationQueue;
        this.logger = logger;
    }

    public async Task<Result<UserResponse>> SelectAsync(
        ExternalUserProfile profile,
        string requestedRole,
        CancellationToken cancellationToken)
    {
        requestedRole = requestedRole.Trim();
        if (!UserRoleNames.IsSelfServiceRole(requestedRole))
        {
            return Result<UserResponse>.Failure(
                new Error(ErrorCodes.ValidationFailed, ErrorCodes.ValidationFailed));
        }

        string role = UserRoleNames.NormalizeSelfServiceRole(requestedRole);
        Result<UserResponse> syncResult = await userService.GetOrCreateExternalAsync(
            profile,
            cancellationToken);
        if (syncResult.IsFailure)
        {
            return syncResult;
        }

        string externalSubject = profile.Subject.Trim();
        string? previousRole = syncResult.Value!.Roles.FirstOrDefault(
            UserRoleNames.IsSelfServiceRole);
        ExternalRoleSynchronizationResult synchronizationResult =
            await externalRoleSynchronizer.SynchronizeSelfServiceRoleAsync(
                externalSubject,
                role,
                previousRole,
                cancellationToken);
        if (synchronizationResult != ExternalRoleSynchronizationResult.Succeeded)
        {
            if (synchronizationResult == ExternalRoleSynchronizationResult.ReconciliationRequired)
            {
                await QueueReconciliationAsync(externalSubject, previousRole, role);
            }

            return SynchronizationFailure();
        }

        try
        {
            User? user = await roleWriter.ReplaceAsync(
                profile.Provider.Trim(),
                externalSubject,
                role,
                cancellationToken);
            if (user is not null)
            {
                return Result<UserResponse>.Success(user.ToResponse());
            }

            bool restored = await RestorePreviousExternalRoleAsync(
                externalSubject,
                role,
                previousRole);
            LogReconciliationOutcome(
                "Local role persistence could not find the external user after Auth0 synchronization.",
                restored);
            return SynchronizationFailure();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            bool restored = await RestorePreviousExternalRoleAsync(
                externalSubject,
                role,
                previousRole);
            LogReconciliationOutcome(
                "Local role persistence failed after Auth0 synchronization.",
                restored);
            return SynchronizationFailure();
        }
    }

    private async Task<bool> RestorePreviousExternalRoleAsync(
        string externalSubject,
        string currentRole,
        string? previousRole)
    {
        if (string.IsNullOrWhiteSpace(previousRole) ||
            string.Equals(previousRole, currentRole, StringComparison.Ordinal))
        {
            return true;
        }

        try
        {
            ExternalRoleSynchronizationResult result =
                await externalRoleSynchronizer.SynchronizeSelfServiceRoleAsync(
                    externalSubject,
                    previousRole,
                    currentRole,
                    CancellationToken.None);
            if (result == ExternalRoleSynchronizationResult.Succeeded)
            {
                return true;
            }

            await QueueReconciliationAsync(externalSubject, previousRole, currentRole);
            return false;
        }
        catch (Exception)
        {
            await QueueReconciliationAsync(externalSubject, previousRole, currentRole);
            logger.LogCritical(
                "Auth0 role compensation threw after local role persistence failed. Durable reconciliation is required.");
            return false;
        }
    }

    private async Task QueueReconciliationAsync(
        string externalSubject,
        string? desiredRole,
        string? currentExternalRole)
    {
        if (string.IsNullOrWhiteSpace(desiredRole))
        {
            return;
        }

        try
        {
            await reconciliationQueue.EnqueueAsync(
                externalSubject,
                desiredRole,
                currentExternalRole,
                CancellationToken.None);
        }
        catch (Exception)
        {
            logger.LogCritical("Durable Auth0 role reconciliation could not be queued.");
        }
    }

    private void LogReconciliationOutcome(string failure, bool restored)
    {
        if (restored)
        {
            logger.LogError("{Failure} The previous external role was restored.", failure);
            return;
        }

        logger.LogCritical(
            "{Failure} Previous external role restoration failed; durable reconciliation is required.",
            failure);
    }

    private static Result<UserResponse> SynchronizationFailure() =>
        Result<UserResponse>.Failure(
            new Error(
                IdentityErrorCodes.ExternalRoleSynchronizationFailed,
                IdentityErrorCodes.ExternalRoleSynchronizationFailed));
}
