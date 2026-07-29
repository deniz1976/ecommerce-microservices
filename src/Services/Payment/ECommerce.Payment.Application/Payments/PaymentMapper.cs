namespace ECommerce.Payment.Application.Payments;

public static class PaymentMapper
{
    public static PaymentResponse ToResponse(this Domain.Payment payment)
    {
        return new PaymentResponse(
            payment.Id,
            payment.OrderId,
            payment.CustomerId,
            payment.Amount,
            payment.Currency,
            payment.Status,
            payment.CreatedAt,
            payment.UpdatedAt,
            payment.Transactions
                .OrderBy(transaction => transaction.CreatedAt)
                .Select(
                    transaction => new PaymentTransactionResponse(
                        transaction.Id,
                        transaction.Type,
                        transaction.Amount,
                        transaction.Currency,
                        transaction.CreatedAt))
                .ToArray());
    }
}
