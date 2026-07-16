namespace ECommerce.OrderingSaga.Domain;

public sealed class OrderWorkflowItem
{
    private OrderWorkflowItem()
    {
    }

    public OrderWorkflowItem(Guid workflowId, Guid productId, int quantity)
    {
        Id = Guid.NewGuid();
        WorkflowId = workflowId;
        ProductId = productId;
        Quantity = quantity;
    }

    public Guid Id { get; private set; }

    public Guid WorkflowId { get; private set; }

    public Guid ProductId { get; private set; }

    public int Quantity { get; private set; }
}
