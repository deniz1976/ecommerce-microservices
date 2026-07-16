using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace ECommerce.BuildingBlocks.Security;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseECommerceSecurity(this WebApplication app)
    {
        AuthOptions options = app.Configuration
            .GetSection(AuthOptions.SectionName)
            .Get<AuthOptions>() ?? new AuthOptions();

        if (!string.IsNullOrWhiteSpace(options.Authority) &&
            !string.IsNullOrWhiteSpace(options.Audience))
        {
            app.UseAuthentication();
        }

        app.UseAuthorization();

        return app;
    }
}
