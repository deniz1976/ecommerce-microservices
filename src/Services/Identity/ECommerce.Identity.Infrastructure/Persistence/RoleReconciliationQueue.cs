using ECommerce.Identity.Application.Users;
using ECommerce.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Identity.Infrastructure.Persistence;

public sealed class RoleReconciliationQueue : IRoleReconciliationQueue
{
    private readonly IServiceScopeFactory scopeFactory;

    public RoleReconciliationQueue(IServiceScopeFactory scopeFactory)
    {
        this.scopeFactory = scopeFactory;
    }

    public async Task EnqueueAsync(
        string externalSubject,
        string desiredRole,
        string? currentExternalRole,
        CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        IdentityDbContext dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        bool exists = await dbContext.RoleReconciliationJobs.AnyAsync(
            job =>
                job.CompletedAt == null &&
                job.ExternalSubject == externalSubject &&
                job.DesiredRole == desiredRole,
            cancellationToken);
        if (exists)
        {
            return;
        }

        dbContext.RoleReconciliationJobs.Add(
            new RoleReconciliationJob(
                Guid.NewGuid(),
                externalSubject,
                desiredRole,
                currentExternalRole));
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
