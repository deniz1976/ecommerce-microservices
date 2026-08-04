using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Identity.Application.AdminUsers;

namespace ECommerce.Identity.Application.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler
    : IQueryHandler<GetUserByIdQuery, Result<AdminUserResponse>>
{
    private readonly AdminUserService userService;

    public GetUserByIdQueryHandler(AdminUserService userService)
    {
        this.userService = userService;
    }

    public Task<Result<AdminUserResponse>> HandleAsync(
        GetUserByIdQuery query,
        CancellationToken cancellationToken)
    {
        return userService.GetByIdAsync(query.Id, cancellationToken);
    }
}
