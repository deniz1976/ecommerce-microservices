namespace ECommerce.Payment.Application.Payments;

public sealed class PaymentProviderOperationException : Exception
{
    public PaymentProviderOperationException(string message)
        : base(message)
    {
    }
}
