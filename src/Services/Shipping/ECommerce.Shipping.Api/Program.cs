using ECommerce.BuildingBlocks.EventBus;
using ECommerce.BuildingBlocks.Localization;
using ECommerce.BuildingBlocks.Observability;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Shipping.Application;
using ECommerce.Shipping.Infrastructure;
using ECommerce.Shipping.Infrastructure.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddECommerceLocalization();
builder.Services.AddECommerceObservability(builder.Configuration, "ECommerce.Shipping.Api");
builder.Services.AddOidcReadySecurity(builder.Configuration);
builder.Services.AddShippingApplication();
builder.Services.AddShippingInfrastructure(builder.Configuration);
builder.Services.AddECommerceMassTransit(
    builder.Configuration,
    "shipping",
    [typeof(ECommerce.Shipping.Infrastructure.Messaging.CreateShipmentConsumer).Assembly],
    registration => registration.AddPostgresEntityFrameworkOutbox<ShippingDbContext>());
builder.Services.AddHealthChecks();

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseECommerceSecurity();

app.MapHealthChecks("/health/live").AllowAnonymous();
app.MapHealthChecks("/health/ready").AllowAnonymous();

app.Run();
