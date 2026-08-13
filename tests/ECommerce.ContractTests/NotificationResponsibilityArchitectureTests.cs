using ECommerce.Notification.Application.Commands.MarkAllNotificationsRead;
using ECommerce.Notification.Application.Commands.MarkNotificationRead;
using ECommerce.Notification.Application.Notifications;

namespace ECommerce.ContractTests;

public sealed class NotificationResponsibilityArchitectureTests
{
    [Theory]
    [InlineData(typeof(MarkNotificationReadCommandHandler), typeof(NotificationReadService))]
    [InlineData(typeof(MarkAllNotificationsReadCommandHandler), typeof(BulkNotificationReadService))]
    public void ReadStateHandlersDependOnTheirFocusedService(Type handlerType, Type serviceType)
    {
        System.Reflection.ConstructorInfo constructor = Assert.Single(handlerType.GetConstructors());
        System.Reflection.ParameterInfo parameter = Assert.Single(constructor.GetParameters());
        Assert.Equal(serviceType, parameter.ParameterType);
    }

    [Fact]
    public void NotificationApplicationOperationsAreSplitByResponsibility()
    {
        string notificationDirectory = RepositoryPath(
            "src",
            "Services",
            "Notification",
            "ECommerce.Notification.Application",
            "Notifications");

        Assert.False(File.Exists(
            Path.Combine(notificationDirectory, "NotificationService.cs")));

        string creationService = File.ReadAllText(
            Path.Combine(notificationDirectory, "NotificationCreationService.cs"));
        string readService = File.ReadAllText(
            Path.Combine(notificationDirectory, "NotificationReadService.cs"));
        string bulkReadService = File.ReadAllText(
            Path.Combine(notificationDirectory, "BulkNotificationReadService.cs"));

        Assert.Contains("ILiveNotificationPublisher", creationService, StringComparison.Ordinal);
        Assert.Contains("FindBySourceAsync", creationService, StringComparison.Ordinal);
        Assert.DoesNotContain("MarkRead", creationService, StringComparison.Ordinal);
        Assert.DoesNotContain("TimeProvider", creationService, StringComparison.Ordinal);

        Assert.Contains("MarkReadAsync", readService, StringComparison.Ordinal);
        Assert.DoesNotContain("INotificationReader", readService, StringComparison.Ordinal);
        Assert.Contains("MarkAllReadAsync", bulkReadService, StringComparison.Ordinal);
        Assert.Contains("INotificationReader", bulkReadService, StringComparison.Ordinal);
        Assert.Contains("TimeProvider", readService, StringComparison.Ordinal);
        Assert.DoesNotContain("ILiveNotificationPublisher", readService, StringComparison.Ordinal);
        Assert.DoesNotContain("FindBySourceAsync", readService, StringComparison.Ordinal);
    }

    private static string RepositoryPath(params string[] segments)
    {
        string root = AppContext.BaseDirectory;
        while (!File.Exists(Path.Combine(root, "ECommerce.sln")))
        {
            root = Directory.GetParent(root)?.FullName ??
                throw new DirectoryNotFoundException("Repository root was not found.");
        }

        return Path.Combine([root, .. segments]);
    }
}
