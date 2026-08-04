using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Identity.Application.AdminUsers;

namespace ECommerce.Identity.Application.Queries.SearchAdminUsers;

public sealed class SearchAdminUsersQueryHandler
    : IQueryHandler<SearchAdminUsersQuery, PagedResult<AdminUserResponse>>
{
    private readonly AdminUserService adminUserService;

    public SearchAdminUsersQueryHandler(AdminUserService adminUserService)
    {
        this.adminUserService = adminUserService;
    }

    public Task<PagedResult<AdminUserResponse>> HandleAsync(
        SearchAdminUsersQuery query,
        CancellationToken cancellationToken)
    {
        return adminUserService.SearchAsync(query, cancellationToken);
    }
}
