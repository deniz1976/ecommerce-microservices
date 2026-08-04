using ECommerce.BuildingBlocks.EventBus;
using ECommerce.BuildingBlocks.Localization;
using ECommerce.BuildingBlocks.Observability;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Ordering.Application;
using ECommerce.Ordering.Infrastructure;
using ECommerce.Ordering.Infrastructure.Messaging;
using ECommerce.Ordering.Infrastructure.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
const string serviceName = "ECommerce.Ordering.Api";

builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddECommerceLocalization();
builder.Services.AddECommerceObservability(builder.Configuration, serviceName);
builder.Services.AddOidcReadySecurity(builder.Configuration);
builder.Services.AddCustomerOwnership(builder.Configuration);
builder.Services.AddOrderingApplication();
builder.Services.AddOrderingInfrastructure(builder.Configuration);
builder.Services.AddECommerceMassTransit<OrderingDbContext>(
    builder.Configuration,
    serviceName,
    "ordering",
    [typeof(OrderConfirmedConsumer).Assembly]);
builder.Services.AddHealthChecks();

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseECommerceSecurity();

app.MapHealthChecks("/health/live").AllowAnonymous();
app.MapHealthChecks("/health/ready").AllowAnonymous();
app.MapControllers();

app.Run();
