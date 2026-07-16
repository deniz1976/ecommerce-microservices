using ECommerce.BuildingBlocks.EventBus;
using ECommerce.BuildingBlocks.Observability;
using ECommerce.OrderingSaga.Application;
using ECommerce.OrderingSaga.Infrastructure;
using ECommerce.OrderingSaga.Infrastructure.Persistence;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddECommerceObservability(builder.Configuration, "ECommerce.OrderingSaga.Worker");
builder.Services.AddOrderingSagaApplication();
builder.Services.AddOrderingSagaInfrastructure(builder.Configuration);
builder.Services.AddECommerceMassTransit(
    builder.Configuration,
    "ordering-saga",
    [typeof(ECommerce.OrderingSaga.Worker.Messaging.OrderSubmittedConsumer).Assembly],
    registration => registration.AddPostgresEntityFrameworkOutbox<OrderingSagaDbContext>());

IHost host = builder.Build();
host.Run();
