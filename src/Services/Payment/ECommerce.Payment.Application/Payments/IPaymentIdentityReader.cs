namespace ECommerce.Payment.Application.Payments;

public interface IPaymentIdentityReader
{
    Task<Guid?> FindIdByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);
}
