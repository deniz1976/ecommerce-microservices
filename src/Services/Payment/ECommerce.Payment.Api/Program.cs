using ECommerce.BuildingBlocks.EventBus;
using ECommerce.BuildingBlocks.Localization;
using ECommerce.BuildingBlocks.Observability;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Payment.Application;
using ECommerce.Payment.Api.Payments;
using ECommerce.Payment.Infrastructure;
using ECommerce.Payment.Infrastructure.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
const string serviceName = "ECommerce.Payment.Api";

builder.Services.AddProblemDetails();
builder.Services.AddECommerceLocalization();
builder.Services.AddECommerceObservability(builder.Configuration, serviceName);
builder.Services.AddOidcReadySecurity(builder.Configuration);
builder.Services.AddCustomerOwnership(builder.Configuration);
builder.Services.AddPaymentApplication();
builder.Services.AddPaymentInfrastructure(builder.Configuration);
builder.Services.AddECommerceMassTransit<PaymentDbContext>(
    builder.Configuration,
    serviceName,
    "payment",
    [typeof(ECommerce.Payment.Infrastructure.Messaging.AuthorizePaymentConsumer).Assembly]);
builder.Services.AddHealthChecks();

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseECommerceSecurity();

app.MapHealthChecks("/health/live").AllowAnonymous();
app.MapHealthChecks("/health/ready").AllowAnonymous();
app.MapPaymentEndpoints();

app.Run();
