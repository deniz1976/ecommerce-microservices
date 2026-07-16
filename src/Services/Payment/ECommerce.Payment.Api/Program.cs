using ECommerce.BuildingBlocks.EventBus;
using ECommerce.BuildingBlocks.Localization;
using ECommerce.BuildingBlocks.Observability;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Payment.Application;
using ECommerce.Payment.Infrastructure;
using ECommerce.Payment.Infrastructure.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddECommerceLocalization();
builder.Services.AddECommerceObservability(builder.Configuration, "ECommerce.Payment.Api");
builder.Services.AddOidcReadySecurity(builder.Configuration);
builder.Services.AddPaymentApplication();
builder.Services.AddPaymentInfrastructure(builder.Configuration);
builder.Services.AddECommerceMassTransit(
    builder.Configuration,
    [typeof(ECommerce.Payment.Infrastructure.Messaging.AuthorizePaymentConsumer).Assembly],
    registration => registration.AddPostgresEntityFrameworkOutbox<PaymentDbContext>());
builder.Services.AddHealthChecks();

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseECommerceSecurity();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.Run();
