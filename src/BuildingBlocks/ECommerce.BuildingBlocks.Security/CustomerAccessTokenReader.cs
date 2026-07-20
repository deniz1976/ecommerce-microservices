using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace ECommerce.BuildingBlocks.Security;

public static class CustomerAccessTokenReader
{
    public static string? Read(HttpContext? httpContext)
    {
        string? authorization = httpContext?.Request.Headers.Authorization;
        if (AuthenticationHeaderValue.TryParse(authorization, out AuthenticationHeaderValue? header) &&
            string.Equals(header.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(header.Parameter))
        {
            return header.Parameter;
        }

        PathString path = httpContext?.Request.Path ?? PathString.Empty;
        if (path.StartsWithSegments("/hubs/notifications") &&
            httpContext!.Request.Query.TryGetValue(
                "access_token",
                out Microsoft.Extensions.Primitives.StringValues queryToken))
        {
            return queryToken.ToString();
        }

        return null;
    }
}
