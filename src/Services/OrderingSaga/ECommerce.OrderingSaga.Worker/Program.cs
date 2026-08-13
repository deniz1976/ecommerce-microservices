using ECommerce.BuildingBlocks.EventBus;
using ECommerce.BuildingBlocks.Observability;
using ECommerce.OrderingSaga.Application;
using ECommerce.OrderingSaga.Infrastructure;
using ECommerce.OrderingSaga.Infrastructure.Persistence;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
const string serviceName = "ECommerce.OrderingSaga.Worker";

builder.Services.AddECommerceObservability(builder.Configuration, serviceName);
builder.Services.AddOrderingSagaApplication();
builder.Services.AddOrderingSagaInfrastructure(builder.Configuration);
builder.Services.AddHostedService<ECommerce.OrderingSaga.Worker.OrderWorkflowTimeoutHostedService>();
builder.Services.AddECommerceMassTransit<OrderingSagaDbContext>(
    builder.Configuration,
    serviceName,
    "ordering-saga",
    [typeof(ECommerce.OrderingSaga.Worker.Messaging.OrderSubmittedConsumer).Assembly]);

IHost host = builder.Build();
host.Run();
