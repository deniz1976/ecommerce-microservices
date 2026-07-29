using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Identity.Application.Queries.SearchAdminUsers;

namespace ECommerce.Identity.Application.AdminUsers;

public interface IAdminUserReader
{
    Task<PagedResult<AdminUserResponse>> SearchAsync(
        SearchAdminUsersQuery query,
        CancellationToken cancellationToken);
}
