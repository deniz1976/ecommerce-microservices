using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Localization;
using ECommerce.BuildingBlocks.Security;
using Microsoft.AspNetCore.SignalR;

namespace ECommerce.Notification.Api.Hubs;

public sealed class NotificationsHub : Hub
{
    private readonly ICustomerOwnershipAuthorizer ownershipAuthorizer;
    private readonly IErrorMessageLocalizer errorMessageLocalizer;

    public NotificationsHub(
        ICustomerOwnershipAuthorizer ownershipAuthorizer,
        IErrorMessageLocalizer errorMessageLocalizer)
    {
        this.ownershipAuthorizer = ownershipAuthorizer;
        this.errorMessageLocalizer = errorMessageLocalizer;
    }

    public async Task JoinCustomerGroup(string customerId)
    {
        Guid parsedCustomerId = ParseCustomerId(customerId);
        if (!await CanAccessAsync(parsedCustomerId))
        {
            throw CreateHubException(ErrorCodes.AccessDenied);
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, CustomerGroupName(parsedCustomerId));
    }

    public async Task LeaveCustomerGroup(string customerId)
    {
        Guid parsedCustomerId = ParseCustomerId(customerId);
        if (!await CanAccessAsync(parsedCustomerId))
        {
            throw CreateHubException(ErrorCodes.AccessDenied);
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

    private Guid ParseCustomerId(string customerId)
    {
        return Guid.TryParse(customerId, out Guid parsedCustomerId)
            ? parsedCustomerId
            : throw CreateHubException(ErrorCodes.ValidationFailed);
    }

    private HubException CreateHubException(string errorCode)
    {
        string culture = Context
            .GetHttpContext()?
            .Request
            .Headers
            .AcceptLanguage
            .ToString() ?? string.Empty;
        string message = errorMessageLocalizer.GetMessage(errorCode, culture);
        return new HubException($"{errorCode}: {message}");
    }
}
