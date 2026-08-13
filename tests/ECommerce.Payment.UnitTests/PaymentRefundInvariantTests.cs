using ECommerce.Payment.Domain;
using PaymentEntity = ECommerce.Payment.Domain.Payment;

namespace ECommerce.Payment.UnitTests;

public sealed class PaymentRefundInvariantTests
{
    [Fact]
    public void MarkRefundedRejectsMismatchedAmountWithoutChangingState()
    {
        PaymentEntity payment = CreateAuthorizedPayment();
        DateTimeOffset updatedAt = payment.UpdatedAt;
        int transactionCount = payment.Transactions.Count;

        PaymentRefundEligibility result = payment.MarkRefunded(
            payment.CustomerId,
            payment.Amount + 1m,
            payment.Currency,
            "refund-reference",
            "Compensation");

        Assert.Equal(PaymentRefundEligibility.AmountMismatch, result);
        Assert.Equal(PaymentStatus.Authorized, payment.Status);
        Assert.Equal(updatedAt, payment.UpdatedAt);
        Assert.Equal(transactionCount, payment.Transactions.Count);
    }

    [Fact]
    public void MarkRefundedRejectsMismatchedCustomerWithoutChangingState()
    {
        PaymentEntity payment = CreateAuthorizedPayment();

        PaymentRefundEligibility result = payment.MarkRefunded(
            Guid.NewGuid(),
            payment.Amount,
            payment.Currency,
            "refund-reference",
            "Compensation");

        Assert.Equal(PaymentRefundEligibility.CustomerMismatch, result);
        Assert.Equal(PaymentStatus.Authorized, payment.Status);
        Assert.Single(payment.Transactions);
    }

    [Fact]
    public void MarkRefundedRejectsMismatchedCurrencyWithoutChangingState()
    {
        PaymentEntity payment = CreateAuthorizedPayment();

        PaymentRefundEligibility result = payment.MarkRefunded(
            payment.CustomerId,
            payment.Amount,
            "USD",
            "refund-reference",
            "Compensation");

        Assert.Equal(PaymentRefundEligibility.CurrencyMismatch, result);
        Assert.Equal(PaymentStatus.Authorized, payment.Status);
        Assert.Single(payment.Transactions);
    }

    [Fact]
    public void MarkRefundedAcceptsCaseInsensitiveMatchingCurrency()
    {
        PaymentEntity payment = CreateAuthorizedPayment();

        PaymentRefundEligibility result = payment.MarkRefunded(
            payment.CustomerId,
            payment.Amount,
            payment.Currency.ToLowerInvariant(),
            "refund-reference",
            "Compensation");

        Assert.Equal(PaymentRefundEligibility.Eligible, result);
        Assert.Equal(PaymentStatus.Refunded, payment.Status);
        Assert.Equal(2, payment.Transactions.Count);
    }

    private static PaymentEntity CreateAuthorizedPayment()
    {
        return PaymentEntity.CreateAuthorized(
            Guid.NewGuid(),
            Guid.NewGuid(),
            25m,
            "TRY",
            "TestProvider",
            "payment-reference",
            "authorization-reference");
    }
}
