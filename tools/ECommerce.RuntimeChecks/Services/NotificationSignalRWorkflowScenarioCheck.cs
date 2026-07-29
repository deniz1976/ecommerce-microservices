using ECommerce.RuntimeChecks.Configuration;
using ECommerce.RuntimeChecks.Models;
using ECommerce.RuntimeChecks.Probes;

namespace ECommerce.RuntimeChecks.Services;

internal sealed class NotificationSignalRWorkflowScenarioCheck : IWorkflowScenarioCheck
{
    private readonly WorkflowScenarioContext context;
    private readonly INotificationLiveDeliveryProbe liveDeliveryProbe;

    public NotificationSignalRWorkflowScenarioCheck(
        WorkflowScenarioContext context,
        INotificationLiveDeliveryProbe liveDeliveryProbe)
    {
        this.context = context;
        this.liveDeliveryProbe = liveDeliveryProbe;
    }

    public WorkflowScenario Scenario => WorkflowScenario.NotificationSignalR;

    public bool IncludeInAll => false;

    public async Task RunAsync(Guid customerId, CancellationToken cancellationToken)
    {
        Console.WriteLine("Preparing SignalR notification delivery scenario inventory.");
        Guid productId = Guid.NewGuid();
        await context.UpsertInventoryAsync(productId, 25, cancellationToken);

        DateTimeOffset deadline = context.CreateDeadline();
        RuntimeNotificationMessage notification =
            await liveDeliveryProbe.CaptureOrderNotificationAsync(
                customerId,
                token => context.CreateOrderAsync(
                    customerId,
                    productId,
                    10.50m,
                    "Notification SignalR",
                    "34008",
                    token),
                deadline,
                cancellationToken);

        AssertNotificationContract(notification, customerId);
        Console.WriteLine(
            $"Runtime probe passed: SignalR notification delivery ({notification.OrderId})");

        Guid orderId = notification.OrderId!.Value;
        await context.WaitForExpectedValueAsync(
            "SignalR scenario order confirmation",
            "ConnectionStrings__OrderingDb",
            "select status from orders where id = @order_id",
            orderId,
            "Confirmed",
            deadline,
            cancellationToken);
        await context.WaitForExpectedValueAsync(
            "SignalR scenario saga completion",
            "ConnectionStrings__OrderingSagaDb",
            "select status from order_workflows where order_id = @order_id",
            orderId,
            "Completed",
            deadline,
            cancellationToken);
        await context.AssertOrderPresentationAsync(
            orderId,
            customerId,
            RuntimeOrderStatus.Confirmed,
            "Workflow Check Notification SignalR",
            "34008",
            cancellationToken);
    }

    private static void AssertNotificationContract(
        RuntimeNotificationMessage notification,
        Guid expectedCustomerId)
    {
        if (notification.Id == Guid.Empty ||
            notification.CustomerId != expectedCustomerId ||
            notification.OrderId is null ||
            notification.OrderId == Guid.Empty ||
            string.IsNullOrWhiteSpace(notification.Type) ||
            string.IsNullOrWhiteSpace(notification.Title) ||
            string.IsNullOrWhiteSpace(notification.Message) ||
            string.IsNullOrWhiteSpace(notification.Culture) ||
            notification.CreatedAt.Offset != TimeSpan.Zero ||
            notification.ReadAt is not null)
        {
            throw new InvalidOperationException(
                "SignalR notification did not match the expected unread, UTC order-notification contract.");
        }
    }
}
