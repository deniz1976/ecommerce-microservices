using ECommerce.BuildingBlocks.EventBus;
using ECommerce.BuildingBlocks.Localization;
using ECommerce.BuildingBlocks.Observability;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Inventory.Api.Inventory;
using ECommerce.Inventory.Application;
using ECommerce.Inventory.Infrastructure;
using ECommerce.Inventory.Infrastructure.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddECommerceLocalization();
builder.Services.AddECommerceObservability(builder.Configuration, "ECommerce.Inventory.Api");
builder.Services.AddOidcReadySecurity(builder.Configuration);
builder.Services.AddInventoryApplication();
builder.Services.AddInventoryInfrastructure(builder.Configuration);
builder.Services.AddECommerceMassTransit(
    builder.Configuration,
    "inventory",
    [typeof(ECommerce.Inventory.Infrastructure.Messaging.ReserveInventoryConsumer).Assembly],
    registration => registration.AddPostgresEntityFrameworkOutbox<InventoryDbContext>());
builder.Services.AddHealthChecks();

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseECommerceSecurity();

app.MapHealthChecks("/health/live").AllowAnonymous();
app.MapHealthChecks("/health/ready").AllowAnonymous();
app.MapInventoryEndpoints();

app.Run();
