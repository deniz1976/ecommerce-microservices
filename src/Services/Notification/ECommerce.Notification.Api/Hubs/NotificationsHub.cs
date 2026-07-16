using Microsoft.AspNetCore.SignalR;

namespace ECommerce.Notification.Api.Hubs;

public sealed class NotificationsHub : Hub
{
    public Task JoinCustomerGroup(string customerId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, CustomerGroupName(customerId));
    }

    public Task LeaveCustomerGroup(string customerId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, CustomerGroupName(customerId));
    }

    public static string CustomerGroupName(Guid customerId)
    {
        return $"customer:{customerId:N}";
    }

    private static string CustomerGroupName(string customerId)
    {
        return Guid.TryParse(customerId, out Guid parsedCustomerId)
            ? CustomerGroupName(parsedCustomerId)
            : $"customer:{customerId}";
    }
}
