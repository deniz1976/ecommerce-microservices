using ECommerce.BuildingBlocks.Contracts.Events;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.OrderingSaga.Domain;

namespace ECommerce.OrderingSaga.Application.Workflows;

public sealed class OrderSubmissionWorkflowService(
    IRepository<OrderWorkflow, Guid> repository,
    IUnitOfWork unitOfWork,
    OrderWorkflowLoader loader,
    IWorkflowCommandPublisher publisher)
{
    public async Task HandleAsync(
        OrderSubmitted message,
        DateTimeOffset now,
        TimeSpan inventoryTimeout,
        CancellationToken cancellationToken)
    {
        if (await loader.LoadByOrderIdAsync(message.OrderId, cancellationToken) is not null)
        {
            return;
        }

        OrderWorkflow workflow = new(
            message.OrderId, message.CustomerId, message.TotalAmount, message.Currency,
            message.RecipientName, message.AddressLine, message.City, message.CountryCode,
            message.PostalCode, message.CorrelationId, now.Add(inventoryTimeout));
        foreach (ECommerce.BuildingBlocks.Contracts.Orders.OrderLine item in message.Items)
        {
            workflow.AddItem(item.ProductId, item.Quantity);
        }

        repository.Add(workflow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await publisher.ReserveInventoryAsync(
            workflow, message.CorrelationId, message.MessageId, cancellationToken);
    }
}
