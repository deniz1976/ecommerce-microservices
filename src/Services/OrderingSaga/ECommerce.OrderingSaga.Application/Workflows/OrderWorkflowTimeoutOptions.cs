namespace ECommerce.OrderingSaga.Application.Workflows;

public sealed class OrderWorkflowTimeoutOptions
{
    public const string SectionName = "OrderWorkflowTimeouts";

    public int InventorySeconds { get; init; } = 120;

    public int PaymentSeconds { get; init; } = 120;

    public int ShippingSeconds { get; init; } = 180;

    public int PollIntervalSeconds { get; init; } = 5;

    public int BatchSize { get; init; } = 25;

    public TimeSpan InventoryTimeout => TimeSpan.FromSeconds(InventorySeconds);

    public TimeSpan PaymentTimeout => TimeSpan.FromSeconds(PaymentSeconds);

    public TimeSpan ShippingTimeout => TimeSpan.FromSeconds(ShippingSeconds);

    public TimeSpan PollInterval => TimeSpan.FromSeconds(PollIntervalSeconds);
}
