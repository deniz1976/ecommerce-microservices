using System.Text.RegularExpressions;
using ECommerce.BuildingBlocks.Contracts.Events;
using MassTransit;

namespace ECommerce.ContractTests;

public sealed partial class NotificationConsumerCoverageTests
{
    [Fact]
    public void EveryCustomerFacingOrderEventHasANotificationConsumer()
    {
        Type[] consumedEvents = typeof(ECommerce.Notification.Api.Messaging.OrderSubmittedConsumer).Assembly
            .GetTypes()
            .SelectMany(type => type.GetInterfaces())
            .Where(contract =>
                contract.IsGenericType &&
                contract.GetGenericTypeDefinition() == typeof(IConsumer<>))
            .Select(contract => contract.GetGenericArguments()[0])
            .Distinct()
            .ToArray();

        Type[] expected =
        [
            typeof(OrderSubmitted),
            typeof(OrderConfirmed),
            typeof(OrderCancelled),
            typeof(OrderCancellationRequested),
            typeof(OrderCancellationRejected),
            typeof(PaymentAuthorized),
            typeof(PaymentFailed),
            typeof(ShipmentCreated),
            typeof(ShipmentFailed),
        ];

        foreach (Type expectedEvent in expected)
        {
            Assert.Contains(expectedEvent, consumedEvents);
        }
    }

    [Fact]
    public void EveryNotificationTypeIsLocalizedInTheFrontend()
    {
        string root = FindRepositoryRoot();
        string messagingDirectory = Path.Combine(
            root,
            "src",
            "Services",
            "Notification",
            "ECommerce.Notification.Api",
            "Messaging");

        string[] publishedTypes = Directory
            .GetFiles(messagingDirectory, "*Consumer.cs")
            .SelectMany(file => NotificationTypeLiteral()
                .Matches(File.ReadAllText(file))
                .Select(match => match.Groups[1].Value))
            .Distinct()
            .ToArray();

        Assert.NotEmpty(publishedTypes);

        string localization = File.ReadAllText(Path.Combine(
            root,
            "src",
            "Frontend",
            "lib",
            "i18n",
            "notifications.ts"));

        foreach (string publishedType in publishedTypes)
        {
            Assert.Contains($"\"{publishedType}\":", localization, StringComparison.Ordinal);
        }
    }

    [GeneratedRegex("\"((?:order|payment|shipment)\\.[a-z]+)\"")]
    private static partial Regex NotificationTypeLiteral();

    private static string FindRepositoryRoot()
    {
        string root = AppContext.BaseDirectory;
        while (!File.Exists(Path.Combine(root, "ECommerce.sln")))
        {
            root = Directory.GetParent(root)?.FullName ??
                throw new DirectoryNotFoundException("Repository root was not found.");
        }

        return root;
    }
}
