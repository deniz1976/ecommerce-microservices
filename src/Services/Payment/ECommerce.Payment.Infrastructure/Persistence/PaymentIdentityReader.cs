using ECommerce.Payment.Application.Payments;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Payment.Infrastructure.Persistence;

public sealed class PaymentIdentityReader : IPaymentIdentityReader
{
    private readonly PaymentDbContext dbContext;

    public PaymentIdentityReader(PaymentDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<Guid?> FindIdByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        return dbContext.Payments
            .AsNoTracking()
            .Where(payment => payment.OrderId == orderId)
            .Select(payment => (Guid?)payment.Id)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
