namespace ECommerce.Payment.Domain;

public sealed class PaymentTransaction
{
    private PaymentTransaction()
    {
        Currency = string.Empty;
    }

    public PaymentTransaction(
        Guid paymentId,
        PaymentTransactionType type,
        decimal amount,
        string currency,
        string? providerTransactionReference,
        string? reason)
    {
        Id = Guid.NewGuid();
        PaymentId = paymentId;
        Type = type;
        Amount = amount;
        Currency = currency;
        ProviderTransactionReference = providerTransactionReference;
        Reason = reason;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid PaymentId { get; private set; }

    public PaymentTransactionType Type { get; private set; }

    public decimal Amount { get; private set; }

    public string Currency { get; private set; }

    public string? ProviderTransactionReference { get; private set; }

    public string? Reason { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
}
