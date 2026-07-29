using ECommerce.RuntimeChecks.Configuration;
using ECommerce.RuntimeChecks.Models;
using ECommerce.RuntimeChecks.Probes;

namespace ECommerce.RuntimeChecks.Services;

internal sealed class BasketCheckoutWorkflowScenarioCheck : IWorkflowScenarioCheck
{
    private const string Currency = "USD";
    private const string ProductName = "Runtime Checkout Product";
    private const decimal UnitPrice = 12.75m;
    private const int Quantity = 2;

    private readonly WorkflowScenarioContext context;
    private readonly ICatalogProductFixture catalogProductFixture;

    public BasketCheckoutWorkflowScenarioCheck(
        WorkflowScenarioContext context,
        ICatalogProductFixture catalogProductFixture)
    {
        this.context = context;
        this.catalogProductFixture = catalogProductFixture;
    }

    public WorkflowScenario Scenario => WorkflowScenario.BasketCheckout;

    public async Task RunAsync(Guid customerId, CancellationToken cancellationToken)
    {
        Guid productId = Guid.NewGuid();
        await catalogProductFixture.CreateAsync(
            productId,
            ProductName,
            UnitPrice,
            Currency,
            cancellationToken);

        try
        {
            await RunCheckoutAsync(customerId, productId, cancellationToken);
        }
        finally
        {
            await catalogProductFixture.DeleteAsync(productId, CancellationToken.None);
        }
    }

    private async Task RunCheckoutAsync(
        Guid customerId,
        Guid productId,
        CancellationToken cancellationToken)
    {
        Console.WriteLine("Preparing canonical Basket checkout scenario.");
        await context.UpsertInventoryAsync(productId, 25, cancellationToken);

        RuntimeBasketResponse basket = await context.AddBasketItemAsync(
            customerId,
            productId,
            Quantity,
            cancellationToken);
        AssertCanonicalBasket(basket, customerId, productId);

        Guid checkoutId = Guid.NewGuid();
        RuntimeCheckoutBasketRequest request = new(
            checkoutId,
            "Workflow Check Basket Checkout",
            "Runtime Avenue 1",
            "Istanbul",
            "TR",
            "34000");

        RuntimeCheckoutBasketResponse firstCheckout = await context.CheckoutBasketAsync(
            customerId,
            request,
            cancellationToken);
        RuntimeCheckoutBasketResponse duplicateCheckout = await context.CheckoutBasketAsync(
            customerId,
            request,
            cancellationToken);
        AssertIdempotentCheckout(firstCheckout, duplicateCheckout, checkoutId, customerId);

        DateTimeOffset deadline = context.CreateDeadline();
        await context.WaitForExpectedValueAsync(
            "Basket checkout order confirmation",
            "ConnectionStrings__OrderingDb",
            "select status from orders where id = @order_id",
            checkoutId,
            "Confirmed",
            deadline,
            cancellationToken);
        await context.WaitForExpectedValueAsync(
            "Basket checkout saga completion",
            "ConnectionStrings__OrderingSagaDb",
            "select status from order_workflows where order_id = @order_id",
            checkoutId,
            "Completed",
            deadline,
            cancellationToken);

        OrderResponse order = await context.AssertOrderPresentationAsync(
            checkoutId,
            customerId,
            RuntimeOrderStatus.Confirmed,
            request.RecipientName,
            request.PostalCode,
            cancellationToken);
        AssertCanonicalOrder(order, checkoutId, productId);
        Console.WriteLine("Runtime probe passed: canonical and idempotent Basket checkout handoff");
    }

    private static void AssertCanonicalBasket(
        RuntimeBasketResponse basket,
        Guid customerId,
        Guid productId)
    {
        RuntimeBasketItemResponse? item = basket.Items.SingleOrDefault();
        decimal expectedTotal = UnitPrice * Quantity;

        if (basket.CustomerId != customerId ||
            !string.Equals(basket.Currency, Currency, StringComparison.Ordinal) ||
            basket.TotalAmount != expectedTotal ||
            basket.UpdatedAt.Offset != TimeSpan.Zero ||
            item is null ||
            item.ProductId != productId ||
            !string.Equals(item.ProductName, ProductName, StringComparison.Ordinal) ||
            item.Quantity != Quantity ||
            item.UnitPrice != UnitPrice ||
            item.TotalPrice != expectedTotal ||
            !string.Equals(item.Currency, Currency, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Basket did not use the canonical Catalog product name, price, currency, or UTC timestamp.");
        }
    }

    private static void AssertIdempotentCheckout(
        RuntimeCheckoutBasketResponse first,
        RuntimeCheckoutBasketResponse duplicate,
        Guid checkoutId,
        Guid customerId)
    {
        decimal expectedTotal = UnitPrice * Quantity;

        if (first.SnapshotId != checkoutId ||
            first.CustomerId != customerId ||
            first.TotalAmount != expectedTotal ||
            !string.Equals(first.Currency, Currency, StringComparison.Ordinal) ||
            first.CreatedAt.Offset != TimeSpan.Zero ||
            duplicate != first)
        {
            throw new InvalidOperationException(
                "Retrying the same checkoutId did not return the original canonical checkout snapshot.");
        }
    }

    private static void AssertCanonicalOrder(
        OrderResponse order,
        Guid checkoutId,
        Guid productId)
    {
        RuntimeOrderItemResponse? item = order.Items.SingleOrDefault();
        decimal expectedTotal = UnitPrice * Quantity;

        if (order.Id != checkoutId ||
            !string.Equals(order.Currency, Currency, StringComparison.Ordinal) ||
            order.TotalAmount != expectedTotal ||
            item is null ||
            item.ProductId != productId ||
            !string.Equals(item.ProductName, ProductName, StringComparison.Ordinal) ||
            item.Quantity != Quantity ||
            item.UnitPrice != UnitPrice ||
            item.TotalPrice != expectedTotal ||
            !string.Equals(item.Currency, Currency, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Ordering did not preserve the canonical Basket checkout item and total.");
        }
    }
}
