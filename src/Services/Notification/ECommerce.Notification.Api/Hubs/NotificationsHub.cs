using ECommerce.BuildingBlocks.Security;
using Microsoft.AspNetCore.SignalR;

namespace ECommerce.Notification.Api.Hubs;

public sealed class NotificationsHub : Hub
{
    private readonly ICustomerOwnershipAuthorizer ownershipAuthorizer;

    public NotificationsHub(ICustomerOwnershipAuthorizer ownershipAuthorizer)
    {
        this.ownershipAuthorizer = ownershipAuthorizer;
    }

    public async Task JoinCustomerGroup(string customerId)
    {
        Guid parsedCustomerId = ParseCustomerId(customerId);
        if (!await CanAccessAsync(parsedCustomerId))
        {
            throw new HubException("Access to the requested customer notification group is denied.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, CustomerGroupName(parsedCustomerId));
    }

    public async Task LeaveCustomerGroup(string customerId)
    {
        Guid parsedCustomerId = ParseCustomerId(customerId);
        if (!await CanAccessAsync(parsedCustomerId))
        {
            throw new HubException("Access to the requested customer notification group is denied.");
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, CustomerGroupName(parsedCustomerId));
    }

    public static string CustomerGroupName(Guid customerId)
    {
        return $"customer:{customerId:N}";
    }

    private Task<bool> CanAccessAsync(Guid customerId)
    {
        HttpContext? httpContext = Context.GetHttpContext();
        return ownershipAuthorizer.CanAccessAsync(
            customerId,
            Context.User,
            CustomerAccessTokenReader.Read(httpContext),
            Context.ConnectionAborted);
    }

    private static Guid ParseCustomerId(string customerId)
    {
        return Guid.TryParse(customerId, out Guid parsedCustomerId)
            ? parsedCustomerId
            : throw new HubException("Customer id must be a valid GUID.");
    }
}
