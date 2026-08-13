using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Identity.Application.AdminUsers;
using ECommerce.Identity.Application.Commands.GetOrCreateExternalUser;
using ECommerce.Identity.Application.Commands.RegisterUser;
using ECommerce.Identity.Application.Commands.SelectExternalUserRole;
using ECommerce.Identity.Application.Queries.GetUserById;
using ECommerce.Identity.Application.Queries.SearchAdminUsers;
using ECommerce.Identity.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddSingleton<IExternalRoleSynchronizer, DisabledExternalRoleSynchronizer>();
        services.AddSingleton<IRoleReconciliationQueue, DisabledRoleReconciliationQueue>();
        services.AddScoped<UserRegistrationService>();
        services.AddScoped<ExternalUserProvisioningService>();
        services.AddScoped<IExternalUserProvisioningService>(
            provider => provider.GetRequiredService<ExternalUserProvisioningService>());
        services.AddScoped<AdminUserService>();
        services.AddScoped<
            ICommandHandler<RegisterUserCommand, Result<UserResponse>>,
            RegisterUserCommandHandler>();
        services.AddScoped<
            ICommandHandler<GetOrCreateExternalUserCommand, Result<UserResponse>>,
            GetOrCreateExternalUserCommandHandler>();
        services.AddScoped<
            ICommandHandler<SelectExternalUserRoleCommand, Result<UserResponse>>,
            SelectExternalUserRoleCommandHandler>();
        services.AddScoped<
            IQueryHandler<GetUserByIdQuery, Result<AdminUserResponse>>,
            GetUserByIdQueryHandler>();
        services.AddScoped<
            IQueryHandler<SearchAdminUsersQuery, PagedResult<AdminUserResponse>>,
            SearchAdminUsersQueryHandler>();
        return services;
    }
}
