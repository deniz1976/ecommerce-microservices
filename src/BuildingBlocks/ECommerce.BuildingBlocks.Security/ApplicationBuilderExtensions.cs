using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace ECommerce.BuildingBlocks.Security;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseECommerceAuthentication(this WebApplication app)
    {
        AuthOptions options = ReadOptions(app);

        if (IsConfigured(options))
        {
            app.UseAuthentication();
        }

        return app;
    }

    public static WebApplication UseECommerceSecurity(this WebApplication app)
    {
        app.UseECommerceAuthentication();

        app.UseAuthorization();

        return app;
    }

    private static AuthOptions ReadOptions(WebApplication app)
    {
        return app.Configuration
            .GetSection(AuthOptions.SectionName)
            .Get<AuthOptions>() ?? new AuthOptions();
    }

    private static bool IsConfigured(AuthOptions options)
    {
        return AuthConfigurationValidator.IsConfigured(options);
    }
}
