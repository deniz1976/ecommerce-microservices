namespace ECommerce.Payment.Domain;

public sealed class Payment
{
    private readonly List<PaymentTransaction> transactions = [];

    private Payment()
    {
        Currency = string.Empty;
    }

    private Payment(
        Guid orderId,
        Guid customerId,
        decimal amount,
        string currency,
        PaymentStatus status,
        string providerName,
        string? providerPaymentReference,
        string? failureReason)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        CustomerId = customerId;
        Amount = amount;
        Currency = currency;
        Status = status;
        ProviderName = providerName;
        ProviderPaymentReference = providerPaymentReference;
        FailureReason = failureReason;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public Guid CustomerId { get; private set; }

    public decimal Amount { get; private set; }

    public string Currency { get; private set; }

    public PaymentStatus Status { get; private set; }

    public string? ProviderName { get; private set; }

    public string? ProviderPaymentReference { get; private set; }

    public string? FailureReason { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<PaymentTransaction> Transactions => transactions;

    public PaymentRefundEligibility EvaluateRefund(
        Guid customerId,
        decimal amount,
        string currency)
    {
        if (Status == PaymentStatus.Refunded)
        {
            return PaymentRefundEligibility.AlreadyRefunded;
        }

        if (Status != PaymentStatus.Authorized)
        {
            return PaymentRefundEligibility.InvalidStatus;
        }

        if (CustomerId != customerId)
        {
            return PaymentRefundEligibility.CustomerMismatch;
        }

        if (Amount != amount)
        {
            return PaymentRefundEligibility.AmountMismatch;
        }

        return string.Equals(Currency, currency, StringComparison.OrdinalIgnoreCase)
            ? PaymentRefundEligibility.Eligible
            : PaymentRefundEligibility.CurrencyMismatch;
    }

    public static Payment CreateAuthorized(
        Guid orderId,
        Guid customerId,
        decimal amount,
        string currency,
        string providerName,
        string providerPaymentReference,
        string providerTransactionReference)
    {
        Payment payment = new(
            orderId,
            customerId,
            amount,
            currency,
            PaymentStatus.Authorized,
            providerName,
            providerPaymentReference,
            null);
        payment.transactions.Add(
            new PaymentTransaction(
                payment.Id,
                PaymentTransactionType.Authorization,
                amount,
                currency,
                providerTransactionReference,
                null));
        return payment;
    }

    public static Payment CreateFailed(
        Guid orderId,
        Guid customerId,
        decimal amount,
        string currency,
        string providerName,
        string reason)
    {
        Payment payment = new(
            orderId,
            customerId,
            amount,
            currency,
            PaymentStatus.Failed,
            providerName,
            null,
            reason);
        payment.transactions.Add(
            new PaymentTransaction(
                payment.Id,
                PaymentTransactionType.Authorization,
                amount,
                currency,
                null,
                reason));
        return payment;
    }

    public PaymentRefundEligibility MarkRefunded(
        Guid customerId,
        decimal amount,
        string currency,
        string providerTransactionReference,
        string reason)
    {
        PaymentRefundEligibility eligibility = EvaluateRefund(
            customerId,
            amount,
            currency);
        if (eligibility != PaymentRefundEligibility.Eligible)
        {
            return eligibility;
        }

        Status = PaymentStatus.Refunded;
        FailureReason = null;
        UpdatedAt = DateTimeOffset.UtcNow;
        transactions.Add(
            new PaymentTransaction(
                Id,
                PaymentTransactionType.Refund,
                amount,
                currency,
                providerTransactionReference,
                reason));
        return PaymentRefundEligibility.Eligible;
    }
}
