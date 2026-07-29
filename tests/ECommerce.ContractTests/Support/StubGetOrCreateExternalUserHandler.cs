using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Identity.Application.Commands.GetOrCreateExternalUser;
using ECommerce.Identity.Application.Users;

namespace ECommerce.ContractTests;

internal sealed class StubGetOrCreateExternalUserHandler
    : ICommandHandler<GetOrCreateExternalUserCommand, Result<UserResponse>>
{
    private readonly Result<UserResponse> result;

    public StubGetOrCreateExternalUserHandler(Result<UserResponse> result)
    {
        this.result = result;
    }

    public Task<Result<UserResponse>> HandleAsync(
        GetOrCreateExternalUserCommand command,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(result);
    }
}
