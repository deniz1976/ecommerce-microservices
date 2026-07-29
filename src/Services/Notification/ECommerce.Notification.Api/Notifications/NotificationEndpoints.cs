using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Notification.Application.Commands.MarkNotificationRead;
using ECommerce.Notification.Application.Notifications;
using ECommerce.Notification.Application.Queries.GetCustomerNotifications;

namespace ECommerce.Notification.Api.Notifications;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/v1/notifications")
            .WithTags("Notifications")
            .RequireAuthorization(AuthorizationPolicies.AuthenticatedUser);

        group.MapGet("/customer/{customerId:guid}", GetByCustomerAsync)
            .WithName("GetNotificationsByCustomer");
        group.MapPut("/customer/{customerId:guid}/{notificationId:guid}/read", MarkReadAsync)
            .WithName("MarkNotificationRead");

        return endpoints;
    }

    private static async Task<IResult> GetByCustomerAsync(
        Guid customerId,
        IQueryHandler<
            GetCustomerNotificationsQuery,
            PagedResult<NotificationMessage>> queryHandler,
        ICustomerOwnershipAuthorizer ownershipAuthorizer,
        HttpContext httpContext,
        int pageNumber = 1,
        int pageSize = 20,
        bool unreadOnly = false,
        CancellationToken cancellationToken = default)
    {
        if (!await ownershipAuthorizer.CanAccessAsync(customerId, cancellationToken))
        {
            return NotificationResults.Forbidden(httpContext);
        }

        PagedResult<NotificationMessage> result = await queryHandler.HandleAsync(
            new GetCustomerNotificationsQuery(
                customerId,
                pageNumber,
                pageSize,
                unreadOnly),
            cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> MarkReadAsync(
        Guid customerId,
        Guid notificationId,
        ICommandHandler<
            MarkNotificationReadCommand,
            Result<NotificationMessage>> commandHandler,
        ICustomerOwnershipAuthorizer ownershipAuthorizer,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!await ownershipAuthorizer.CanAccessAsync(customerId, cancellationToken))
        {
            return NotificationResults.Forbidden(httpContext);
        }

        Result<NotificationMessage> result = await commandHandler.HandleAsync(
            new MarkNotificationReadCommand(customerId, notificationId),
            cancellationToken);
        return NotificationResults.FromResult(result, httpContext);
    }
}
