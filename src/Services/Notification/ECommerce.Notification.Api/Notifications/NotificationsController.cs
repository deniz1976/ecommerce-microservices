using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Notification.Application.Commands.MarkNotificationRead;
using ECommerce.Notification.Application.Commands.MarkAllNotificationsRead;
using ECommerce.Notification.Application.Notifications;
using ECommerce.Notification.Application.Queries.GetCustomerNotifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Notification.Api.Notifications;

[ApiController]
[Route("api/v1/notifications")]
[Authorize(Policy = AuthorizationPolicies.AuthenticatedUser)]
public sealed class NotificationsController(
    ISender sender,
    ICustomerOwnershipAuthorizer ownershipAuthorizer) : ControllerBase
{
    [HttpGet("customer/{customerId:guid}", Name = "GetNotificationsByCustomer")]
    public async Task<IResult> GetByCustomerAsync(
        Guid customerId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool unreadOnly = false,
        CancellationToken cancellationToken = default)
    {
        if (!await ownershipAuthorizer.CanAccessAsync(customerId, cancellationToken))
        {
            return NotificationResults.Forbidden(HttpContext);
        }

        PagedResult<NotificationMessage> result = await sender.Send(
            new GetCustomerNotificationsQuery(
                customerId,
                pageNumber,
                pageSize,
                unreadOnly),
            cancellationToken);
        return Results.Ok(result);
    }

    [HttpPut(
        "customer/{customerId:guid}/{notificationId:guid}/read",
        Name = "MarkNotificationRead")]
    public async Task<IResult> MarkReadAsync(
        Guid customerId,
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        if (!await ownershipAuthorizer.CanAccessAsync(customerId, cancellationToken))
        {
            return NotificationResults.Forbidden(HttpContext);
        }

        Result<NotificationMessage> result = await sender.Send(
            new MarkNotificationReadCommand(customerId, notificationId),
            cancellationToken);
        return NotificationResults.FromResult(result, HttpContext);
    }

    [HttpPut("customer/{customerId:guid}/read", Name = "MarkAllNotificationsRead")]
    public async Task<IResult> MarkAllReadAsync(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        if (!await ownershipAuthorizer.CanAccessAsync(customerId, cancellationToken))
        {
            return NotificationResults.Forbidden(HttpContext);
        }

        int changedCount = await sender.Send(
            new MarkAllNotificationsReadCommand(customerId),
            cancellationToken);
        return Results.Ok(new { changedCount });
    }
}
