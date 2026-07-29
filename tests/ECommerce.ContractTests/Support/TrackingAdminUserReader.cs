using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Identity.Application.AdminUsers;
using ECommerce.Identity.Application.Queries.SearchAdminUsers;

namespace ECommerce.ContractTests.Support;

internal sealed class TrackingAdminUserReader : IAdminUserReader
{
    public SearchAdminUsersQuery? LastQuery { get; private set; }

    public Task<PagedResult<AdminUserResponse>> SearchAsync(
        SearchAdminUsersQuery query,
        CancellationToken cancellationToken)
    {
        LastQuery = query;
        return Task.FromResult(
            new PagedResult<AdminUserResponse>(
                [],
                query.PageNumber,
                query.PageSize,
                0));
    }
}
