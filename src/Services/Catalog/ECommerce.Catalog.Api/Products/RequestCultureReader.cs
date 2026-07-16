using ECommerce.BuildingBlocks.Localization;

namespace ECommerce.Catalog.Api.Products;

public static class RequestCultureReader
{
    public static string Read(HttpContext httpContext)
    {
        string? acceptLanguage = httpContext.Request.Headers.AcceptLanguage.FirstOrDefault();
        return SupportedCultures.Normalize(acceptLanguage);
    }
}
