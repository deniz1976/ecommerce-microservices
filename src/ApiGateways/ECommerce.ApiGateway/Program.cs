using ECommerce.BuildingBlocks.Observability;
using ECommerce.BuildingBlocks.Security;
using ECommerce.ApiGateway.Configuration;
using ECommerce.ApiGateway.Logging;
using ECommerce.ApiGateway.Middleware;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ocelot.Middleware;
using Microsoft.AspNetCore.HttpOverrides;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddGatewayRoutes(builder.Environment);

builder.Services.Replace(
    ServiceDescriptor.Singleton<ILoggerFactory, RedactingLoggerFactory>());
builder.Services.AddProblemDetails();
builder.Services.AddECommerceObservability(builder.Configuration, "ECommerce.ApiGateway");
builder.Services.AddOidcReadyAuthentication(builder.Configuration);
builder.Services.AddGateway(builder.Configuration);
builder.Services.AddGatewayRateLimiting(builder.Configuration);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
});

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseForwardedHeaders();
app.UseWebSockets();
app.UseCors();
app.UseECommerceAuthentication();
app.UseGatewayRateLimiting();
app.UseGatewayHealthChecks();

OcelotPipelineConfiguration ocelotPipeline = new()
{
    PreErrorResponderMiddleware = async (context, next) =>
    {
        await next.Invoke();
        await GatewayAuthorizationErrorResponseWriter.WriteIfNeededAsync(context);
    }
};

await app.UseOcelot(ocelotPipeline);
await app.RunAsync();
