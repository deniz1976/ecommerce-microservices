using ECommerce.Payment.Application.Payments;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Payment.Infrastructure.Persistence;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext dbContext;

    public PaymentRepository(PaymentDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<Domain.Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return dbContext.Payments
            .Include(x => x.Transactions)
            .SingleOrDefaultAsync(x => x.OrderId == orderId, cancellationToken);
    }

    public void Add(Domain.Payment payment)
    {
        dbContext.Payments.Add(payment);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
