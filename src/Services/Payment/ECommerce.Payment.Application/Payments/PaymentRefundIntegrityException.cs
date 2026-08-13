namespace ECommerce.Payment.Application.Payments;

public sealed class PaymentRefundIntegrityException : Exception
{
    public PaymentRefundIntegrityException(string message)
        : base(message)
    {
    }
}
