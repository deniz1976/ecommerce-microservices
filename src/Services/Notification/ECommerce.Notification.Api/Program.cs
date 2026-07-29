using ECommerce.BuildingBlocks.EventBus;
using ECommerce.BuildingBlocks.Localization;
using ECommerce.BuildingBlocks.Observability;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Notification.Api.Hubs;
using ECommerce.Notification.Api.Notifications;
using ECommerce.Notification.Application;
using ECommerce.Notification.Application.Notifications;
using ECommerce.Notification.Infrastructure;
using ECommerce.Notification.Infrastructure.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
const string serviceName = "ECommerce.Notification.Api";

builder.Services.AddProblemDetails();
builder.Services.AddSignalR();
builder.Services.AddECommerceLocalization();
builder.Services.AddECommerceObservability(builder.Configuration, serviceName);
builder.Services.AddOidcReadySecurity(builder.Configuration);
builder.Services.AddCustomerOwnership(builder.Configuration);
builder.Services.AddNotificationApplication();
builder.Services.AddNotificationInfrastructure(builder.Configuration);
builder.Services.AddScoped<ILiveNotificationPublisher, SignalRLiveNotificationPublisher>();
builder.Services.AddECommerceMassTransit<NotificationDbContext>(
    builder.Configuration,
    serviceName,
    "notification",
    [typeof(ECommerce.Notification.Api.Messaging.OrderSubmittedConsumer).Assembly]);
builder.Services.AddHealthChecks();

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseECommerceSecurity();

app.MapHealthChecks("/health/live").AllowAnonymous();
app.MapHealthChecks("/health/ready").AllowAnonymous();
app.MapHub<NotificationsHub>("/hubs/notifications")
    .RequireAuthorization(AuthorizationPolicies.AuthenticatedUser);
app.MapNotificationEndpoints();

app.Run();
