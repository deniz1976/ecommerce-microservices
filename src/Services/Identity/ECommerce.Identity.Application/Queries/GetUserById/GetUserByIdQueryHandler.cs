using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Identity.Application.Users;

namespace ECommerce.Identity.Application.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler
    : IQueryHandler<GetUserByIdQuery, Result<UserResponse>>
{
    private readonly IRepository<Domain.User, Guid> repository;

    public GetUserByIdQueryHandler(IRepository<Domain.User, Guid> repository)
    {
        this.repository = repository;
    }

    public async Task<Result<UserResponse>> HandleAsync(
        GetUserByIdQuery query,
        CancellationToken cancellationToken)
    {
        Domain.User? user = await repository.GetByIdAsync(query.Id, cancellationToken);

        return user is null
            ? Result<UserResponse>.Failure(
                new Error(IdentityErrorCodes.UserNotFound, IdentityErrorCodes.UserNotFound))
            : Result<UserResponse>.Success(user.ToResponse());
    }
}
