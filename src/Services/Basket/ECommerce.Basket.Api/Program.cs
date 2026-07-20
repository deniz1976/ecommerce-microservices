using ECommerce.Basket.Api.Baskets;
using ECommerce.Basket.Application;
using ECommerce.Basket.Infrastructure;
using ECommerce.BuildingBlocks.Localization;
using ECommerce.BuildingBlocks.Observability;
using ECommerce.BuildingBlocks.Security;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddECommerceLocalization();
builder.Services.AddECommerceObservability(builder.Configuration, "ECommerce.Basket.Api");
builder.Services.AddOidcReadySecurity(builder.Configuration);
builder.Services.AddCustomerOwnership(builder.Configuration);
builder.Services.AddBasketApplication();
builder.Services.AddBasketInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseECommerceSecurity();

app.MapHealthChecks("/health/live").AllowAnonymous();
app.MapHealthChecks("/health/ready").AllowAnonymous();
app.MapBasketEndpoints();

app.Run();
