namespace ECommerce.Payment.Domain;

public enum PaymentRefundEligibility
{
    Eligible = 0,
    AlreadyRefunded = 1,
    InvalidStatus = 2,
    CustomerMismatch = 3,
    AmountMismatch = 4,
    CurrencyMismatch = 5
}
