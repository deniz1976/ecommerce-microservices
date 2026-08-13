using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.OrderingSaga.Application.Workflows;
using ECommerce.OrderingSaga.Domain;

namespace ECommerce.Ordering.UnitTests;

public sealed class OrderWorkflowTimeoutTests
{
    private static readonly DateTimeOffset TestNow = new(2026, 8, 13, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Inventory_timeout_cancels_without_compensation()
    {
        (OrderWorkflow workflow, OrderWorkflowTimeoutService service, FakeWorkflowCommandPublisher publisher) =
            CreateService();

        await service.HandleAsync(workflow.Id, TestNow, CancellationToken.None);

        Assert.Equal(OrderWorkflowStatus.Cancelled, workflow.Status);
        Assert.Equal(ErrorCodes.InventoryTimeout, workflow.CancellationReason);
        Assert.Equal(TestNow, workflow.TimeoutHandledAt);
        Assert.Equal(0, publisher.ReleaseCount);
        Assert.Equal(0, publisher.RefundCount);
        Assert.Equal(1, publisher.CancelCount);
        Assert.Equal(ErrorCodes.InventoryTimeout, publisher.CancellationReasonCode);
    }

    [Fact]
    public async Task Payment_timeout_releases_inventory()
    {
        (OrderWorkflow workflow, OrderWorkflowTimeoutService service, FakeWorkflowCommandPublisher publisher) =
            CreateService();
        workflow.MarkInventoryReserved(TestNow);

        await service.HandleAsync(workflow.Id, TestNow, CancellationToken.None);

        Assert.Equal(OrderWorkflowStatus.Cancelled, workflow.Status);
        Assert.Equal(ErrorCodes.PaymentTimeout, workflow.CancellationReason);
        Assert.Equal(1, publisher.ReleaseCount);
        Assert.Equal(0, publisher.RefundCount);
    }

    [Fact]
    public async Task Shipping_timeout_refunds_payment_and_releases_inventory()
    {
        (OrderWorkflow workflow, OrderWorkflowTimeoutService service, FakeWorkflowCommandPublisher publisher) =
            CreateService();
        workflow.MarkInventoryReserved(TestNow.AddMinutes(-1));
        workflow.MarkPaymentAuthorized(TestNow);

        await service.HandleAsync(workflow.Id, TestNow, CancellationToken.None);

        Assert.Equal(OrderWorkflowStatus.Cancelled, workflow.Status);
        Assert.Equal(ErrorCodes.ShipmentTimeout, workflow.CancellationReason);
        Assert.Equal(1, publisher.ReleaseCount);
        Assert.Equal(1, publisher.RefundCount);
    }

    [Fact]
    public async Task Early_or_repeated_timeout_is_ignored()
    {
        (OrderWorkflow workflow, OrderWorkflowTimeoutService service, FakeWorkflowCommandPublisher publisher) =
            CreateService(TestNow.AddMinutes(1));

        await service.HandleAsync(workflow.Id, TestNow, CancellationToken.None);
        await service.HandleAsync(workflow.Id, TestNow.AddMinutes(2), CancellationToken.None);
        await service.HandleAsync(workflow.Id, TestNow.AddMinutes(3), CancellationToken.None);

        Assert.Equal(OrderWorkflowStatus.Cancelled, workflow.Status);
        Assert.Equal(1, publisher.CancelCount);
    }

    private static (
        OrderWorkflow Workflow,
        OrderWorkflowTimeoutService Service,
        FakeWorkflowCommandPublisher Publisher) CreateService(DateTimeOffset? deadline = null)
    {
        OrderWorkflow workflow = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            10m,
            "TRY",
            "Test Customer",
            "Address",
            "Istanbul",
            "TR",
            "34000",
            Guid.NewGuid(),
            deadline ?? TestNow);
        workflow.AddItem(Guid.NewGuid(), 1);
        OrderWorkflowFakeRepository repository = new();
        repository.Add(workflow);
        FakeWorkflowCommandPublisher publisher = new();
        OrderWorkflowTimeoutService service = new(repository, repository, publisher);
        return (workflow, service, publisher);
    }
}
