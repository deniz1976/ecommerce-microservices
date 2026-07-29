using ECommerce.BuildingBlocks.Localization;

namespace ECommerce.Notification.Api.Notifications;

public static class RequestCultureReader
{
    public static string Read(HttpContext httpContext)
    {
        string? requestedCulture = httpContext.Request.Headers.AcceptLanguage
            .FirstOrDefault()?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault()?
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];
        return SupportedCultures.Normalize(requestedCulture);
    }
}
