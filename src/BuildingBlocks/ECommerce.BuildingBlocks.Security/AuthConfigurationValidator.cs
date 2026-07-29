namespace ECommerce.BuildingBlocks.Security;

internal static class AuthConfigurationValidator
{
    public static bool IsConfigured(AuthOptions options)
    {
        bool hasAuthority = !string.IsNullOrWhiteSpace(options.Authority);
        bool hasAudience = !string.IsNullOrWhiteSpace(options.Audience);

        if (hasAuthority != hasAudience)
        {
            throw new InvalidOperationException(
                $"{AuthOptions.SectionName}:Authority and {AuthOptions.SectionName}:Audience must be configured together.");
        }

        if (!hasAuthority)
        {
            return false;
        }

        if (!Uri.TryCreate(options.Authority, UriKind.Absolute, out Uri? authority) ||
            (authority.Scheme != Uri.UriSchemeHttps &&
             authority.Scheme != Uri.UriSchemeHttp) ||
            !string.IsNullOrEmpty(authority.UserInfo) ||
            !string.IsNullOrEmpty(authority.Query) ||
            !string.IsNullOrEmpty(authority.Fragment))
        {
            throw new InvalidOperationException(
                $"{AuthOptions.SectionName}:Authority must be an absolute HTTP or HTTPS URL without credentials, query, or fragment.");
        }

        if (options.RequireHttpsMetadata &&
            authority.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException(
                $"{AuthOptions.SectionName}:Authority must use HTTPS when {AuthOptions.SectionName}:RequireHttpsMetadata is enabled.");
        }

        return true;
    }
}
