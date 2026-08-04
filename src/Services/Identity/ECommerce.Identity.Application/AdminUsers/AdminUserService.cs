using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Identity.Application.Queries.SearchAdminUsers;

namespace ECommerce.Identity.Application.AdminUsers;

public sealed class AdminUserService(IAdminUserReader userReader)
{
    private const int MaximumPageSize = 50;
    private const int MaximumPageNumber = 10_000;
    private const int MaximumSearchLength = 100;
    private const int MaximumRoleLength = 32;

    public Task<PagedResult<AdminUserResponse>> SearchAsync(
        SearchAdminUsersQuery query,
        CancellationToken cancellationToken)
    {
        SearchAdminUsersQuery normalizedQuery = query with
        {
            PageNumber = Math.Clamp(query.PageNumber, 1, MaximumPageNumber),
            PageSize = Math.Clamp(query.PageSize, 1, MaximumPageSize),
            Search = NormalizeOptional(query.Search, MaximumSearchLength),
            Role = NormalizeOptional(query.Role, MaximumRoleLength)
        };

        return userReader.SearchAsync(normalizedQuery, cancellationToken);
    }

    private static string? NormalizeOptional(string? value, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string normalized = value.Trim();
        return normalized.Length <= maximumLength
            ? normalized
            : normalized[..maximumLength];
    }
}
