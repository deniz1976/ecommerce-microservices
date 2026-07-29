using ECommerce.Identity.Domain;

namespace ECommerce.ContractTests;

public sealed class RoleReconciliationJobTests
{
    [Fact]
    public void FailedAttemptsUseBoundedUtcBackoff()
    {
        RoleReconciliationJob job = new(
            Guid.NewGuid(),
            "auth0|subject",
            UserRoleNames.Customer,
            UserRoleNames.Seller);
        DateTimeOffset attemptedAt = DateTimeOffset.UtcNow;

        job.MarkFailed(attemptedAt);

        Assert.Equal(1, job.AttemptCount);
        Assert.Equal(attemptedAt.AddSeconds(30), job.NextAttemptAt);
        Assert.Equal(TimeSpan.Zero, job.NextAttemptAt.Offset);
    }

    [Fact]
    public void FailedAttemptsReachOneHourMaximumBackoff()
    {
        RoleReconciliationJob job = new(
            Guid.NewGuid(),
            "auth0|subject",
            UserRoleNames.Customer,
            UserRoleNames.Seller);
        DateTimeOffset attemptedAt = DateTimeOffset.UtcNow;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            job.MarkFailed(attemptedAt);
        }

        Assert.Equal(attemptedAt.AddHours(1), job.NextAttemptAt);
    }

    [Fact]
    public void SuccessRecordsUtcCompletion()
    {
        RoleReconciliationJob job = new(
            Guid.NewGuid(),
            "auth0|subject",
            UserRoleNames.Seller,
            UserRoleNames.Customer);
        DateTimeOffset completedAt = DateTimeOffset.UtcNow;

        job.MarkSucceeded(completedAt);

        Assert.Equal(completedAt, job.CompletedAt);
        Assert.Equal(TimeSpan.Zero, job.CompletedAt!.Value.Offset);
    }
}
