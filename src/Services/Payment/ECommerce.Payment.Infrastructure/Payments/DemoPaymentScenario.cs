namespace ECommerce.Payment.Infrastructure.Payments;

public enum DemoPaymentScenario
{
    Success = 0,
    Decline = 1,
    TransientFailure = 2,
    RefundFailure = 3
}
