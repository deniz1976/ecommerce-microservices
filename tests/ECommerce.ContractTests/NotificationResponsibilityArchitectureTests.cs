namespace ECommerce.ContractTests;

public sealed class NotificationResponsibilityArchitectureTests
{
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
        string readStateService = File.ReadAllText(
            Path.Combine(notificationDirectory, "NotificationReadStateService.cs"));

        Assert.Contains("ILiveNotificationPublisher", creationService, StringComparison.Ordinal);
        Assert.Contains("FindBySourceAsync", creationService, StringComparison.Ordinal);
        Assert.DoesNotContain("MarkRead", creationService, StringComparison.Ordinal);
        Assert.DoesNotContain("TimeProvider", creationService, StringComparison.Ordinal);

        Assert.Contains("MarkReadAsync", readStateService, StringComparison.Ordinal);
        Assert.Contains("MarkAllReadAsync", readStateService, StringComparison.Ordinal);
        Assert.Contains("TimeProvider", readStateService, StringComparison.Ordinal);
        Assert.DoesNotContain("ILiveNotificationPublisher", readStateService, StringComparison.Ordinal);
        Assert.DoesNotContain("FindBySourceAsync", readStateService, StringComparison.Ordinal);
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
