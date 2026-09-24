using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Workflows;
using ECommerce.OrderingSaga.Domain;

namespace ECommerce.Ordering.UnitTests;

public sealed class LateWorkflowEventTests
{
    private static readonly DateTimeOffset TestNow = new(2026, 9, 24, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Shipment_created_after_shipping_timeout_cancels_the_shipment()
    {
        (OrderWorkflow workflow, OrderWorkflowFakeRepository repository, FakeWorkflowCommandPublisher publisher) =
            CreateWorkflow();
        workflow.MarkInventoryReserved(TestNow.AddMinutes(-2));
        workflow.MarkPaymentAuthorized(TestNow.AddMinutes(-1));
        await new OrderWorkflowTimeoutService(repository, repository, publisher)
            .HandleAsync(workflow.Id, TestNow, CancellationToken.None);
        ShippingWorkflowService service = new(new OrderWorkflowLoader(repository, repository), repository, publisher);

        await service.HandleCreatedAsync(
            new ShipmentCreated(
                Guid.NewGuid(),
                workflow.CorrelationId,
                null,
                TestNow,
                1,
                workflow.OrderId,
                workflow.CustomerId,
                Guid.NewGuid(),
                "EC-LATE-0001"),
            CancellationToken.None);

        Assert.Equal(OrderWorkflowStatus.Cancelled, workflow.Status);
        Assert.Equal(1, publisher.CancelShipmentCount);
        Assert.Equal(0, publisher.ShipCount);
    }

    [Fact]
    public async Task Shipment_failed_after_shipping_timeout_does_not_compensate_twice()
    {
        (OrderWorkflow workflow, OrderWorkflowFakeRepository repository, FakeWorkflowCommandPublisher publisher) =
            CreateWorkflow();
        workflow.MarkInventoryReserved(TestNow.AddMinutes(-2));
        workflow.MarkPaymentAuthorized(TestNow.AddMinutes(-1));
        await new OrderWorkflowTimeoutService(repository, repository, publisher)
            .HandleAsync(workflow.Id, TestNow, CancellationToken.None);
        ShippingWorkflowService service = new(new OrderWorkflowLoader(repository, repository), repository, publisher);

        await service.HandleFailedAsync(
            new ShipmentFailed(
                Guid.NewGuid(),
                workflow.CorrelationId,
                null,
                TestNow,
                1,
                workflow.OrderId,
                workflow.CustomerId,
                "SHIPMENT_FAILED",
                "The carrier rejected the shipment."),
            CancellationToken.None);

        Assert.Equal(1, publisher.RefundCount);
        Assert.Equal(1, publisher.ReleaseCount);
        Assert.Equal(1, publisher.CancelCount);
    }

    [Fact]
    public async Task Payment_failed_after_customer_cancellation_is_ignored()
    {
        (OrderWorkflow workflow, OrderWorkflowFakeRepository repository, FakeWorkflowCommandPublisher publisher) =
            CreateWorkflow();
        workflow.MarkInventoryReserved(TestNow.AddMinutes(2));
        await new OrderWorkflowCancellationService(repository, new OrderWorkflowLoader(repository, repository), publisher)
            .HandleAsync(
                new OrderCancellationRequested(
                    Guid.NewGuid(),
                    workflow.CorrelationId,
                    null,
                    TestNow,
                    1,
                    workflow.OrderId,
                    workflow.CustomerId),
                CancellationToken.None);
        string? reasonCode = publisher.CancellationReasonCode;
        PaymentWorkflowService service = new(new OrderWorkflowLoader(repository, repository), repository, publisher);

        await service.HandleFailedAsync(
            new PaymentFailed(
                Guid.NewGuid(),
                workflow.CorrelationId,
                null,
                TestNow,
                1,
                workflow.OrderId,
                workflow.CustomerId,
                "PAYMENT_DECLINED",
                "The payment was declined."),
            CancellationToken.None);

        Assert.Equal(1, publisher.CancelCount);
        Assert.Equal(1, publisher.ReleaseCount);
        Assert.Equal(reasonCode, publisher.CancellationReasonCode);
    }

    [Fact]
    public async Task Inventory_failure_after_inventory_timeout_is_ignored()
    {
        (OrderWorkflow workflow, OrderWorkflowFakeRepository repository, FakeWorkflowCommandPublisher publisher) =
            CreateWorkflow(TestNow.AddMinutes(-1));
        await new OrderWorkflowTimeoutService(repository, repository, publisher)
            .HandleAsync(workflow.Id, TestNow, CancellationToken.None);
        InventoryWorkflowService service = new(new OrderWorkflowLoader(repository, repository), repository, publisher);

        await service.HandleFailedAsync(
            new InventoryReservationFailed(
                Guid.NewGuid(),
                workflow.CorrelationId,
                null,
                TestNow,
                1,
                workflow.OrderId,
                workflow.CustomerId,
                "INSUFFICIENT_STOCK",
                "Insufficient stock."),
            CancellationToken.None);

        Assert.Equal(1, publisher.CancelCount);
    }

    private static (
        OrderWorkflow Workflow,
        OrderWorkflowFakeRepository Repository,
        FakeWorkflowCommandPublisher Publisher) CreateWorkflow(DateTimeOffset? inventoryDeadline = null)
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
            inventoryDeadline ?? TestNow.AddMinutes(10));
        workflow.AddItem(Guid.NewGuid(), 1);
        OrderWorkflowFakeRepository repository = new();
        repository.Add(workflow);
        return (workflow, repository, new FakeWorkflowCommandPublisher());
    }
}
