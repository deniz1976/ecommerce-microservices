using ECommerce.Identity.Application.Users;
using ECommerce.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Identity.Infrastructure.Persistence;

public sealed class UserRepository : IUserRepository
{
    private readonly IdentityDbContext dbContext;

    public UserRepository(IdentityDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<Domain.User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Users
            .Include(x => x.Roles)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Domain.User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return dbContext.Users
            .Include(x => x.Roles)
            .SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public Task<Domain.User?> GetByExternalIdentityAsync(string provider, string subject, CancellationToken cancellationToken)
    {
        return dbContext.Users
            .Include(x => x.Roles)
            .SingleOrDefaultAsync(x => x.ExternalProvider == provider && x.ExternalSubject == subject, cancellationToken);
    }

    public async Task<Domain.User?> ReplaceSelfServiceRoleAsync(
        string provider,
        string subject,
        string role,
        CancellationToken cancellationToken)
    {
        Guid? userId = await dbContext.Users
            .AsNoTracking()
            .Where(x => x.ExternalProvider == provider && x.ExternalSubject == subject)
            .Select(x => (Guid?)x.Id)
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

        await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO user_roles (id, user_id, role, created_at)
            VALUES ({Guid.NewGuid()}, {userId.Value}, {role}, {DateTimeOffset.UtcNow});
            """, cancellationToken);

        await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE users
            SET updated_at = {DateTimeOffset.UtcNow},
                onboarding_completed_at = {DateTimeOffset.UtcNow}
            WHERE id = {userId.Value};
            """, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        dbContext.ChangeTracker.Clear();

        return await dbContext.Users
            .Include(x => x.Roles)
            .SingleOrDefaultAsync(x => x.Id == userId.Value, cancellationToken);
    }

    public void Add(Domain.User user)
    {
        dbContext.Users.Add(user);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
