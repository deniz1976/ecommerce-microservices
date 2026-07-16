namespace ECommerce.Payment.Application.Payments;

public interface IPaymentRepository
{
    Task<Domain.Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);

    void Add(Domain.Payment payment);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
