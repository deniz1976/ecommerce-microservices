using ECommerce.BuildingBlocks.Localization;

namespace ECommerce.Ordering.Api.Orders;

public static class RequestCultureReader
{
    public static string Read(HttpContext httpContext)
    {
        string? acceptLanguage = httpContext.Request.Headers.AcceptLanguage.FirstOrDefault();
        return SupportedCultures.Normalize(acceptLanguage);
    }
}
