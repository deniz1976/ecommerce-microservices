using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Identity.Application.AdminUsers;

namespace ECommerce.Identity.Application.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid Id) : IQuery<Result<AdminUserResponse>>;
