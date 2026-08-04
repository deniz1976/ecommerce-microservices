using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Identity.Application.AdminUsers;
using ECommerce.Identity.Application.Queries.SearchAdminUsers;
using ECommerce.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Identity.Infrastructure.Persistence;

public sealed class AdminUserReader : IAdminUserReader
{
    private readonly IdentityDbContext dbContext;

    public AdminUserReader(IdentityDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<AdminUserResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Project(dbContext.Users.AsNoTracking().Where(user => user.Id == id))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<AdminUserResponse>> SearchAsync(
        SearchAdminUsersQuery query,
        CancellationToken cancellationToken)
    {
        IQueryable<User> users = dbContext.Users.AsNoTracking();

        if (query.Search is not null)
        {
            string search = query.Search.ToLower();
            users = users.Where(user =>
                user.Email.ToLower().Contains(search) ||
                user.DisplayName.ToLower().Contains(search));
        }

        if (query.Role is not null)
        {
            string role = query.Role;
            users = users.Where(user => user.Roles.Any(userRole => userRole.Role == role));
        }

        if (query.Status is not null)
        {
            users = users.Where(user => user.Status == query.Status);
        }

        long totalCount = await users.LongCountAsync(cancellationToken);
        IQueryable<User> orderedUsers = users
            .OrderByDescending(user => user.CreatedAt)
            .ThenBy(user => user.Email)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize);
        AdminUserResponse[] items = await Project(orderedUsers)
            .ToArrayAsync(cancellationToken);

        return new PagedResult<AdminUserResponse>(
            items,
            query.PageNumber,
            query.PageSize,
            totalCount);
    }

    private static IQueryable<AdminUserResponse> Project(IQueryable<User> users)
    {
        return users.Select(user => new AdminUserResponse(
                user.Id,
                user.Email,
                user.DisplayName,
                user.Roles
                    .OrderBy(userRole => userRole.Role)
                    .Select(userRole => userRole.Role)
                    .ToArray(),
                user.Status,
                user.OnboardingCompletedAt != null,
                user.CreatedAt,
                user.UpdatedAt));
    }
}
