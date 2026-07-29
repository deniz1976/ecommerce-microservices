namespace ECommerce.Identity.Domain;

public sealed class RoleReconciliationJob
{
    private RoleReconciliationJob()
    {
        ExternalSubject = string.Empty;
        DesiredRole = string.Empty;
    }

    public RoleReconciliationJob(
        Guid id,
        string externalSubject,
        string desiredRole,
        string? currentExternalRole)
    {
        Id = id;
        ExternalSubject = externalSubject;
        DesiredRole = UserRoleNames.NormalizeSelfServiceRole(desiredRole);
        CurrentExternalRole = string.IsNullOrWhiteSpace(currentExternalRole)
            ? null
            : UserRoleNames.NormalizeSelfServiceRole(currentExternalRole);
        CreatedAt = DateTimeOffset.UtcNow;
        NextAttemptAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    public string ExternalSubject { get; private set; }

    public string DesiredRole { get; private set; }

    public string? CurrentExternalRole { get; private set; }

    public int AttemptCount { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset NextAttemptAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public void MarkSucceeded(DateTimeOffset completedAt)
    {
        CompletedAt = completedAt;
    }

    public void MarkFailed(DateTimeOffset attemptedAt)
    {
        AttemptCount++;
        int delaySeconds = Math.Min(3600, 30 * (1 << Math.Min(AttemptCount - 1, 7)));
        NextAttemptAt = attemptedAt.AddSeconds(delaySeconds);
    }
}
