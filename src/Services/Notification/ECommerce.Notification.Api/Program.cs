using ECommerce.BuildingBlocks.EventBus;
using ECommerce.BuildingBlocks.Localization;
using ECommerce.BuildingBlocks.Observability;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Notification.Api.Hubs;
using ECommerce.Notification.Application;
using ECommerce.Notification.Application.Notifications;
using ECommerce.Notification.Infrastructure;
using ECommerce.Notification.Infrastructure.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddSignalR();
builder.Services.AddECommerceLocalization();
builder.Services.AddECommerceObservability(builder.Configuration, "ECommerce.Notification.Api");
builder.Services.AddOidcReadySecurity(builder.Configuration);
builder.Services.AddNotificationApplication();
builder.Services.AddNotificationInfrastructure(builder.Configuration);
builder.Services.AddScoped<ILiveNotificationPublisher, SignalRLiveNotificationPublisher>();
builder.Services.AddECommerceMassTransit(
    builder.Configuration,
    [typeof(ECommerce.Notification.Api.Messaging.OrderSubmittedConsumer).Assembly],
    registration => registration.AddPostgresEntityFrameworkOutbox<NotificationDbContext>());
builder.Services.AddHealthChecks();

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseECommerceSecurity();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");
app.MapHub<NotificationsHub>("/hubs/notifications");

app.Run();
