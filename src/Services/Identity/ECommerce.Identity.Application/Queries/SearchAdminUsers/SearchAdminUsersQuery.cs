using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Identity.Application.AdminUsers;
using ECommerce.Identity.Domain;

namespace ECommerce.Identity.Application.Queries.SearchAdminUsers;

public sealed record SearchAdminUsersQuery(
    int PageNumber = 1,
    int PageSize = 20,
    string? Search = null,
    string? Role = null,
    UserStatus? Status = null) : IQuery<PagedResult<AdminUserResponse>>;
