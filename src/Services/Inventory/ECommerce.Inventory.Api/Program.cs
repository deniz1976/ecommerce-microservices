using ECommerce.BuildingBlocks.EventBus;
using ECommerce.BuildingBlocks.Localization;
using ECommerce.BuildingBlocks.Observability;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Inventory.Application;
using ECommerce.Inventory.Infrastructure;
using ECommerce.Inventory.Infrastructure.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
const string serviceName = "ECommerce.Inventory.Api";

builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddECommerceLocalization();
builder.Services.AddECommerceObservability(builder.Configuration, serviceName);
builder.Services.AddOidcReadySecurity(builder.Configuration);
builder.Services.AddInventoryApplication();
builder.Services.AddInventoryInfrastructure(builder.Configuration);
builder.Services.AddECommerceMassTransit<InventoryDbContext>(
    builder.Configuration,
    serviceName,
    "inventory",
    [typeof(ECommerce.Inventory.Infrastructure.Messaging.ReserveInventoryConsumer).Assembly]);
builder.Services.AddHealthChecks();

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseECommerceSecurity();

app.MapHealthChecks("/health/live").AllowAnonymous();
app.MapHealthChecks("/health/ready").AllowAnonymous();
app.MapControllers();

app.Run();
