using ECommerce.BuildingBlocks.Observability;
using ECommerce.BuildingBlocks.Security;
using ECommerce.ApiGateway.Configuration;
using ECommerce.ApiGateway.Middleware;
using Ocelot.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddGatewayRoutes(builder.Environment);

builder.Services.AddProblemDetails();
builder.Services.AddECommerceObservability(builder.Configuration, "ECommerce.ApiGateway");
builder.Services.AddOidcReadySecurity(builder.Configuration);
builder.Services.AddGateway(builder.Configuration);

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseWebSockets();
app.UseCors();
app.UseECommerceSecurity();
app.UseGatewayHealthChecks();

await app.UseOcelot();
await app.RunAsync();
