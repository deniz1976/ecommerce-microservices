using System.Text.Json;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Localization;
using ECommerce.Basket.Api.Baskets;
using ECommerce.Identity.Api.Errors;
using ECommerce.Identity.Application;
using ECommerce.Inventory.Api.Errors;
using ECommerce.Notification.Api.Notifications;
using ECommerce.Ordering.Api.Orders;
using ECommerce.Payment.Api.Payments;
using ECommerce.Shipping.Api.Shipments;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.ContractTests;

public sealed class LocalizedApiErrorTests
{
    [Theory]
    [InlineData("basket")]
    [InlineData("ordering")]
    [InlineData("notification")]
    public async Task OwnershipDenialUsesLocalizedApiErrorContract(string service)
    {
        await using ServiceProvider services = CreateServices();
        DefaultHttpContext context = CreateContext(services, "tr");
        IResult result = service switch
        {
            "basket" => BasketResults.Forbidden(context),
            "ordering" => OrderResults.Forbidden(context),
            "notification" => NotificationResults.Forbidden(context),
            _ => throw new ArgumentOutOfRangeException(nameof(service))
        };

        await result.ExecuteAsync(context);

        using JsonDocument response = await ReadResponseAsync(context);
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.Equal(ErrorCodes.AccessDenied, response.RootElement.GetProperty("code").GetString());
        Assert.Equal(
            "İstenen kaynağa erişim reddedildi.",
            response.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task IdentityErrorUsesRequestedTurkishCulture()
    {
        await using ServiceProvider services = CreateServices();
        DefaultHttpContext context = CreateContext(services, "tr-TR,tr;q=0.9");
        Result<object> failure = Result<object>.Failure(
            new Error(IdentityErrorCodes.UserNotFound, IdentityErrorCodes.UserNotFound));

        await IdentityResults.FromResult(failure, context).ExecuteAsync(context);

        using JsonDocument response = await ReadResponseAsync(context);
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.Equal(IdentityErrorCodes.UserNotFound, response.RootElement.GetProperty("code").GetString());
        Assert.Equal("Kullanıcı bulunamadı.", response.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task InventoryValidationDetailsUseRequestedEnglishCulture()
    {
        await using ServiceProvider services = CreateServices();
        DefaultHttpContext context = CreateContext(services, "en-US");

        await InventoryResults.InvalidQuantity(context).ExecuteAsync(context);

        using JsonDocument response = await ReadResponseAsync(context);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal(ErrorCodes.ValidationFailed, response.RootElement.GetProperty("code").GetString());
        Assert.Equal(
            "Quantity on hand must be zero or greater.",
            response.RootElement.GetProperty("details").GetProperty("quantityOnHand")[0].GetString());
    }

    [Fact]
    public async Task UnsupportedCultureFallsBackToEnglish()
    {
        await using ServiceProvider services = CreateServices();
        DefaultHttpContext context = CreateContext(services, "de-DE");
        Result<object> failure = Result<object>.Failure(
            new Error(IdentityErrorCodes.UserNotFound, IdentityErrorCodes.UserNotFound));

        await IdentityResults.FromResult(failure, context).ExecuteAsync(context);

        using JsonDocument response = await ReadResponseAsync(context);
        Assert.Equal("User was not found.", response.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task PaymentNotFoundUsesRequestedTurkishCulture()
    {
        await using ServiceProvider services = CreateServices();
        DefaultHttpContext context = CreateContext(services, "tr");
        Result<object> failure = Result<object>.Failure(
            new Error(ErrorCodes.PaymentNotFound, ErrorCodes.PaymentNotFound));

        await PaymentResults.FromResult(failure, context).ExecuteAsync(context);

        using JsonDocument response = await ReadResponseAsync(context);
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.Equal("Ödeme bulunamadı.", response.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task ShipmentNotFoundUsesRequestedTurkishCulture()
    {
        await using ServiceProvider services = CreateServices();
        DefaultHttpContext context = CreateContext(services, "tr");
        Result<object> failure = Result<object>.Failure(
            new Error(ErrorCodes.ShipmentNotFound, ErrorCodes.ShipmentNotFound));

        await ShippingResults.FromResult(failure, context).ExecuteAsync(context);

        using JsonDocument response = await ReadResponseAsync(context);
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.Equal("Kargo kaydı bulunamadı.", response.RootElement.GetProperty("message").GetString());
    }

    [Theory]
    [InlineData("en", "Notification was not found.")]
    [InlineData("tr", "Bildirim bulunamadı.")]
    public async Task NotificationNotFoundUsesRequestedCulture(string culture, string expectedMessage)
    {
        await using ServiceProvider services = CreateServices();
        DefaultHttpContext context = CreateContext(services, culture);
        Result<object> failure = Result<object>.Failure(
            new Error(
                ErrorCodes.NotificationNotFound,
                ErrorCodes.NotificationNotFound));

        await NotificationResults.FromResult(failure, context).ExecuteAsync(context);

        using JsonDocument response = await ReadResponseAsync(context);
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.Equal(expectedMessage, response.RootElement.GetProperty("message").GetString());
    }

    private static ServiceProvider CreateServices()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddRouting();
        services.AddECommerceLocalization();
        return services.BuildServiceProvider();
    }

    private static DefaultHttpContext CreateContext(IServiceProvider services, string acceptLanguage)
    {
        DefaultHttpContext context = new()
        {
            RequestServices = services
        };
        context.Request.Headers.AcceptLanguage = acceptLanguage;
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<JsonDocument> ReadResponseAsync(DefaultHttpContext context)
    {
        context.Response.Body.Position = 0;
        return await JsonDocument.ParseAsync(context.Response.Body);
    }
}
