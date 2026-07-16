namespace ECommerce.ApiGateway.Middleware;

public static class GatewayHealthCheckExtensions
{
    public static IApplicationBuilder UseGatewayHealthChecks(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            if (context.Request.Path.Equals("/health/live", StringComparison.OrdinalIgnoreCase) ||
                context.Request.Path.Equals("/health/ready", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsync("Healthy");
                return;
            }

            await next();
        });
    }
}
