using ECommerce.BuildingBlocks.EventBus;
using ECommerce.BuildingBlocks.Observability;
using ECommerce.BuildingBlocks.Localization;
using ECommerce.BuildingBlocks.Security;
using ECommerce.OrderingSaga.Application;
using ECommerce.OrderingSaga.Infrastructure;
using ECommerce.OrderingSaga.Infrastructure.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
const string serviceName = "ECommerce.OrderingSaga.Worker";

builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddECommerceLocalization();
builder.Services.AddECommerceObservability(builder.Configuration, serviceName);
builder.Services.AddOidcReadySecurity(builder.Configuration);
builder.Services.AddOrderingSagaApplication();
builder.Services.AddOrderingSagaInfrastructure(builder.Configuration);
builder.Services.AddHostedService<ECommerce.OrderingSaga.Worker.OrderWorkflowTimeoutHostedService>();
builder.Services.AddECommerceMassTransit<OrderingSagaDbContext>(
    builder.Configuration,
    serviceName,
    "ordering-saga",
    [typeof(ECommerce.OrderingSaga.Worker.Messaging.OrderSubmittedConsumer).Assembly]);

builder.Services.AddHealthChecks();

WebApplication app = builder.Build();
app.UseExceptionHandler();
app.UseECommerceSecurity();
app.MapHealthChecks("/health/live").AllowAnonymous();
app.MapHealthChecks("/health/ready").AllowAnonymous();
app.MapControllers();
app.Run();
