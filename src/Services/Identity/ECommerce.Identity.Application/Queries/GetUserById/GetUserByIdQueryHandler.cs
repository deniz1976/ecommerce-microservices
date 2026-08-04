using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Identity.Application.Users;

namespace ECommerce.Identity.Application.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler
    : IQueryHandler<GetUserByIdQuery, Result<UserResponse>>
{
    private readonly UserService userService;

    public GetUserByIdQueryHandler(UserService userService)
    {
        this.userService = userService;
    }

    public Task<Result<UserResponse>> HandleAsync(
        GetUserByIdQuery query,
        CancellationToken cancellationToken)
    {
        return userService.GetByIdAsync(query.Id, cancellationToken);
    }
}
