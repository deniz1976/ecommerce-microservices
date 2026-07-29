using ECommerce.Identity.Application.Users;
using ECommerce.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Identity.Infrastructure.Persistence;

public sealed class SelfServiceRoleWriter : ISelfServiceRoleWriter
{
    private readonly IdentityDbContext dbContext;

    public SelfServiceRoleWriter(IdentityDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<User?> ReplaceAsync(
        string provider,
        string subject,
        string role,
        CancellationToken cancellationToken)
    {
        Guid? userId = await dbContext.Users
            .AsNoTracking()
            .Where(user =>
                user.ExternalProvider == provider &&
                user.ExternalSubject == subject)
            .Select(user => (Guid?)user.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (userId is null)
        {
            return null;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            DELETE FROM user_roles
            WHERE user_id = {userId.Value}
              AND role IN ({UserRoleNames.Customer}, {UserRoleNames.Seller});
            """, cancellationToken);

        DateTimeOffset changedAt = DateTimeOffset.UtcNow;
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO user_roles (id, user_id, role, created_at)
            VALUES ({Guid.NewGuid()}, {userId.Value}, {role}, {changedAt});
            """, cancellationToken);

        await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE users
            SET updated_at = {changedAt},
                onboarding_completed_at = {changedAt}
            WHERE id = {userId.Value};
            """, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        dbContext.ChangeTracker.Clear();

        return await dbContext.Users
            .Include(user => user.Roles)
            .SingleOrDefaultAsync(user => user.Id == userId.Value, cancellationToken);
    }
}
