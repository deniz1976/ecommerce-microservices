using ECommerce.Identity.Domain;
using ECommerce.Identity.Infrastructure.Persistence;

namespace ECommerce.ContractTests;

public sealed class RoleReconciliationRetentionPolicyTests
{
    [Fact]
    public void GetCutoff_normalizes_to_utc()
    {
        DateTimeOffset now = new(2026, 7, 28, 15, 0, 0, TimeSpan.FromHours(3));

        DateTimeOffset cutoff =
            RoleReconciliationRetentionPolicy.GetCutoff(now, 30);

        Assert.Equal(new DateTimeOffset(2026, 6, 28, 12, 0, 0, TimeSpan.Zero), cutoff);
        Assert.Equal(TimeSpan.Zero, cutoff.Offset);
    }

    [Fact]
    public void SelectExpiredJobIds_excludes_pending_and_recent_jobs()
    {
        DateTimeOffset cutoff = new(2026, 6, 28, 12, 0, 0, TimeSpan.Zero);
        RoleReconciliationJob expired = CreateJob();
        expired.MarkSucceeded(cutoff.AddSeconds(-1));
        RoleReconciliationJob boundary = CreateJob();
        boundary.MarkSucceeded(cutoff);
        RoleReconciliationJob recent = CreateJob();
        recent.MarkSucceeded(cutoff.AddSeconds(1));
        RoleReconciliationJob pending = CreateJob();

        Guid[] selected = RoleReconciliationRetentionPolicy
            .SelectExpiredJobIds(
                new[] { recent, pending, boundary, expired }.AsQueryable(),
                cutoff,
                100)
            .ToArray();

        Assert.Equal(new[] { expired.Id, boundary.Id }, selected);
    }

    [Fact]
    public void SelectExpiredJobIds_limits_batch_to_oldest_completed_jobs()
    {
        DateTimeOffset cutoff = new(2026, 6, 28, 12, 0, 0, TimeSpan.Zero);
        RoleReconciliationJob oldest = CreateJob();
        oldest.MarkSucceeded(cutoff.AddDays(-2));
        RoleReconciliationJob older = CreateJob();
        older.MarkSucceeded(cutoff.AddDays(-1));

        Guid[] selected = RoleReconciliationRetentionPolicy
            .SelectExpiredJobIds(
                new[] { older, oldest }.AsQueryable(),
                cutoff,
                1)
            .ToArray();

        Assert.Equal(new[] { oldest.Id }, selected);
    }

    private static RoleReconciliationJob CreateJob()
    {
        return new RoleReconciliationJob(
            Guid.NewGuid(),
            $"auth0|{Guid.NewGuid():N}",
            UserRoleNames.Customer,
            UserRoleNames.Seller);
    }
}
