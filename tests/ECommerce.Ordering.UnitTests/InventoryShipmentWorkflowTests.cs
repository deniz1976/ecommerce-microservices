using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Workflows;
using ECommerce.OrderingSaga.Domain;

namespace ECommerce.Ordering.UnitTests;

public sealed class InventoryShipmentWorkflowTests
{
    private static readonly DateTimeOffset TestNow = new(2026, 9, 22, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Created_shipment_decreases_stock_and_confirms_the_order()
    {
        (OrderWorkflow workflow, ShippingWorkflowService service, FakeWorkflowCommandPublisher publisher) =
            CreateService();
        workflow.MarkInventoryReserved(TestNow);
        workflow.MarkPaymentAuthorized(TestNow);

        await service.HandleCreatedAsync(CreatedMessage(workflow), CancellationToken.None);

        Assert.Equal(OrderWorkflowStatus.Completed, workflow.Status);
        Assert.Equal(1, publisher.ShipCount);
        Assert.Equal(0, publisher.ReleaseCount);
    }

    [Fact]
    public async Task Failed_shipment_releases_stock_instead_of_decreasing_it()
    {
        (OrderWorkflow workflow, ShippingWorkflowService service, FakeWorkflowCommandPublisher publisher) =
            CreateService();
        workflow.MarkInventoryReserved(TestNow);
        workflow.MarkPaymentAuthorized(TestNow);

        await service.HandleFailedAsync(FailedMessage(workflow), CancellationToken.None);

        Assert.Equal(OrderWorkflowStatus.Cancelled, workflow.Status);
        Assert.Equal(0, publisher.ShipCount);
        Assert.Equal(1, publisher.ReleaseCount);
        Assert.Equal(1, publisher.RefundCount);
    }

    [Fact]
    public async Task Repeated_shipment_events_decrease_stock_once()
    {
        (OrderWorkflow workflow, ShippingWorkflowService service, FakeWorkflowCommandPublisher publisher) =
            CreateService();
        workflow.MarkInventoryReserved(TestNow);
        workflow.MarkPaymentAuthorized(TestNow);
        ShipmentCreated message = CreatedMessage(workflow);

        await service.HandleCreatedAsync(message, CancellationToken.None);
        await service.HandleCreatedAsync(message, CancellationToken.None);

        Assert.Equal(1, publisher.ShipCount);
    }

    [Fact]
    public async Task Shipment_timeout_never_decreases_stock()
    {
        OrderWorkflow workflow = CreateWorkflow(TestNow);
        workflow.MarkInventoryReserved(TestNow.AddMinutes(-1));
        workflow.MarkPaymentAuthorized(TestNow);
        OrderWorkflowFakeRepository repository = new();
        repository.Add(workflow);
        FakeWorkflowCommandPublisher publisher = new();
        OrderWorkflowTimeoutService service = new(repository, repository, publisher);

        await service.HandleAsync(workflow.Id, TestNow, CancellationToken.None);

        Assert.Equal(OrderWorkflowStatus.Cancelled, workflow.Status);
        Assert.Equal(0, publisher.ShipCount);
        Assert.Equal(1, publisher.ReleaseCount);
    }

    private static ShipmentCreated CreatedMessage(OrderWorkflow workflow) =>
        new(
            Guid.NewGuid(),
            workflow.CorrelationId,
            null,
            TestNow,
            1,
            workflow.OrderId,
            workflow.CustomerId,
            Guid.NewGuid(),
            "EC-TEST-0001");

    private static ShipmentFailed FailedMessage(OrderWorkflow workflow) =>
        new(
            Guid.NewGuid(),
            workflow.CorrelationId,
            null,
            TestNow,
            1,
            workflow.OrderId,
            workflow.CustomerId,
            "SHIPMENT_FAILED",
            "The carrier rejected the shipment.");

    private static OrderWorkflow CreateWorkflow(DateTimeOffset deadline)
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
            deadline);
        workflow.AddItem(Guid.NewGuid(), 1);
        return workflow;
    }

    private static (
        OrderWorkflow Workflow,
        ShippingWorkflowService Service,
        FakeWorkflowCommandPublisher Publisher) CreateService()
    {
        OrderWorkflow workflow = CreateWorkflow(TestNow.AddMinutes(10));
        OrderWorkflowFakeRepository repository = new();
        repository.Add(workflow);
        FakeWorkflowCommandPublisher publisher = new();
        OrderWorkflowLoader loader = new(repository, repository);
        ShippingWorkflowService service = new(loader, repository, publisher);
        return (workflow, service, publisher);
    }
}
