using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.OrderingSaga.Application.Workflows;
using ECommerce.OrderingSaga.Domain;

namespace ECommerce.Ordering.UnitTests;

public sealed class OrderWorkflowCancellationTests
{
    private static readonly DateTimeOffset TestNow = new(2026, 8, 13, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Submitted_workflow_cancels_without_compensation()
    {
        (OrderWorkflow workflow, OrderWorkflowCancellationService service, FakeWorkflowCommandPublisher publisher) =
            CreateService();

        await service.HandleAsync(CreateRequest(workflow), CancellationToken.None);

        Assert.Equal(OrderWorkflowStatus.Cancelled, workflow.Status);
        Assert.Equal(0, publisher.ReleaseCount);
        Assert.Equal(0, publisher.RefundCount);
        Assert.Equal(1, publisher.CancelCount);
    }

    [Fact]
    public async Task Missing_workflow_uses_specific_not_ready_exception()
    {
        OrderWorkflowFakeRepository repository = new();
        OrderWorkflowCancellationService service = new(
            repository,
            repository,
            repository,
            new FakeWorkflowCommandPublisher());
        OrderCancellationRequested request = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            TestNow,
            1,
            Guid.NewGuid(),
            Guid.NewGuid());

        await Assert.ThrowsAsync<OrderWorkflowNotReadyException>(() =>
            service.HandleAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task Inventory_reserved_workflow_releases_stock()
    {
        (OrderWorkflow workflow, OrderWorkflowCancellationService service, FakeWorkflowCommandPublisher publisher) =
            CreateService();
        workflow.MarkInventoryReserved(TestNow.AddMinutes(2));

        await service.HandleAsync(CreateRequest(workflow), CancellationToken.None);

        Assert.Equal(OrderWorkflowStatus.Cancelled, workflow.Status);
        Assert.Equal(1, publisher.ReleaseCount);
        Assert.Equal(0, publisher.RefundCount);
        Assert.Equal(1, publisher.CancelCount);
    }

    [Fact]
    public async Task Payment_authorized_workflow_rejects_late_cancellation()
    {
        (OrderWorkflow workflow, OrderWorkflowCancellationService service, FakeWorkflowCommandPublisher publisher) =
            CreateService();
        workflow.MarkInventoryReserved(TestNow.AddMinutes(2));
        workflow.MarkPaymentAuthorized(TestNow.AddMinutes(4));

        await service.HandleAsync(CreateRequest(workflow), CancellationToken.None);

        Assert.Equal(OrderWorkflowStatus.PaymentAuthorized, workflow.Status);
        Assert.Equal(1, publisher.RejectCancellationCount);
        Assert.Equal(0, publisher.ReleaseCount);
        Assert.Equal(0, publisher.RefundCount);
        Assert.Equal(0, publisher.CancelCount);
    }

    [Fact]
    public async Task Completed_workflow_rejects_late_cancellation()
    {
        (OrderWorkflow workflow, OrderWorkflowCancellationService service, FakeWorkflowCommandPublisher publisher) =
            CreateService();
        workflow.MarkInventoryReserved(TestNow.AddMinutes(2));
        workflow.MarkPaymentAuthorized(TestNow.AddMinutes(4));
        workflow.MarkShipmentCreated();
        workflow.MarkCompleted();

        await service.HandleAsync(CreateRequest(workflow), CancellationToken.None);

        Assert.Equal(OrderWorkflowStatus.Completed, workflow.Status);
        Assert.Equal(1, publisher.RejectCancellationCount);
        Assert.Equal(0, publisher.ReleaseCount);
        Assert.Equal(0, publisher.RefundCount);
        Assert.Equal(0, publisher.CancelCount);
    }

    [Fact]
    public async Task Late_inventory_success_after_customer_cancellation_releases_stock()
    {
        (OrderWorkflow workflow, OrderWorkflowCancellationService cancellationService, FakeWorkflowCommandPublisher publisher) =
            CreateService();
        await cancellationService.HandleAsync(CreateRequest(workflow), CancellationToken.None);

        OrderWorkflowService service = CreateWorkflowService(workflow, publisher);

        await service.HandleAsync(
            new InventoryReserved(
                Guid.NewGuid(),
                Guid.NewGuid(),
                null,
                DateTimeOffset.UtcNow,
                1,
                workflow.OrderId,
                workflow.CustomerId),
            TestNow,
            TimeSpan.FromMinutes(2),
            CancellationToken.None);

        Assert.Equal(1, publisher.ReleaseCount);
        Assert.Equal(0, publisher.RefundCount);
    }

    [Fact]
    public async Task Late_payment_success_after_customer_cancellation_refunds_and_releases()
    {
        (OrderWorkflow workflow, OrderWorkflowCancellationService cancellationService, FakeWorkflowCommandPublisher publisher) =
            CreateService();
        await cancellationService.HandleAsync(CreateRequest(workflow), CancellationToken.None);

        OrderWorkflowService service = CreateWorkflowService(workflow, publisher);

        await service.HandleAsync(
            new PaymentAuthorized(
                Guid.NewGuid(),
                Guid.NewGuid(),
                null,
                DateTimeOffset.UtcNow,
                1,
                workflow.OrderId,
                workflow.CustomerId,
                Guid.NewGuid(),
                workflow.TotalAmount,
                workflow.Currency),
            TestNow,
            TimeSpan.FromMinutes(2),
            CancellationToken.None);

        Assert.Equal(1, publisher.ReleaseCount);
        Assert.Equal(1, publisher.RefundCount);
    }

    private static (
        OrderWorkflow Workflow,
        OrderWorkflowCancellationService Service,
        FakeWorkflowCommandPublisher Publisher) CreateService()
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
            TestNow.AddMinutes(1));
        workflow.AddItem(Guid.NewGuid(), 1);
        OrderWorkflowFakeRepository repository = new();
        repository.Add(workflow);
        FakeWorkflowCommandPublisher publisher = new();
        OrderWorkflowCancellationService service = new(
            repository,
            repository,
            repository,
            publisher);
        return (workflow, service, publisher);
    }

    private static OrderWorkflowService CreateWorkflowService(
        OrderWorkflow workflow,
        FakeWorkflowCommandPublisher publisher)
    {
        OrderWorkflowFakeRepository repository = new();
        repository.Add(workflow);
        return new OrderWorkflowService(repository, repository, repository, publisher);
    }

    private static OrderCancellationRequested CreateRequest(OrderWorkflow workflow)
    {
        return new OrderCancellationRequested(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            DateTimeOffset.UtcNow,
            1,
            workflow.OrderId,
            workflow.CustomerId);
    }
}
