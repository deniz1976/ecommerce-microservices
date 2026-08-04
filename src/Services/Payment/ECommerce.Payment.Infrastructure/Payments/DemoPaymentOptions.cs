namespace ECommerce.Payment.Infrastructure.Payments;

public sealed class DemoPaymentOptions
{
    public const string SectionName = "DemoPayment";

    public DemoPaymentScenario Scenario { get; set; } = DemoPaymentScenario.Success;

    public int AuthorizationDelayMilliseconds { get; set; }
}
