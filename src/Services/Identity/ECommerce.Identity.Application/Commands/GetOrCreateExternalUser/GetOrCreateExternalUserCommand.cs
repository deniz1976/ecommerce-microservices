using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Identity.Application.Users;

namespace ECommerce.Identity.Application.Commands.GetOrCreateExternalUser;

public sealed record GetOrCreateExternalUserCommand(
    ExternalUserProfile Profile) : ICommand<Result<UserResponse>>;
