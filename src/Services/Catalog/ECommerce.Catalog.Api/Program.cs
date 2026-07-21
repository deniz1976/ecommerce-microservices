using ECommerce.BuildingBlocks.Localization;
using ECommerce.BuildingBlocks.Observability;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Catalog.Api.Products;
using ECommerce.Catalog.Api.References;
using ECommerce.Catalog.Api.Stores;
using ECommerce.Catalog.Application;
using ECommerce.Catalog.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddECommerceLocalization();
builder.Services.AddECommerceObservability(builder.Configuration, "ECommerce.Catalog.Api");
builder.Services.AddOidcReadySecurity(builder.Configuration);
builder.Services.AddAuthenticatedUserResolution(builder.Configuration);
builder.Services.AddCatalogApplication();
builder.Services.AddCatalogInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseECommerceSecurity();

app.MapHealthChecks("/health/live").AllowAnonymous();
app.MapHealthChecks("/health/ready").AllowAnonymous();
app.MapProductEndpoints();
app.MapCatalogReferenceEndpoints();
app.MapStoreEndpoints();

app.Run();
