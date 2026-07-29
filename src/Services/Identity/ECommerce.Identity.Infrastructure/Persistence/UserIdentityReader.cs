using ECommerce.Identity.Application.Users;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Identity.Infrastructure.Persistence;

public sealed class UserIdentityReader : IUserIdentityReader
{
    private readonly IdentityDbContext dbContext;

    public UserIdentityReader(IdentityDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<Guid?> FindIdByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        return dbContext.Users
            .AsNoTracking()
            .Where(user => user.Email == email)
            .Select(user => (Guid?)user.Id)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<Guid?> FindIdByExternalIdentityAsync(
        string provider,
        string subject,
        CancellationToken cancellationToken)
    {
        return dbContext.Users
            .AsNoTracking()
            .Where(user =>
                user.ExternalProvider == provider &&
                user.ExternalSubject == subject)
            .Select(user => (Guid?)user.Id)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
