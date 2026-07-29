using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.Payment.Application.Payments;

namespace ECommerce.Payment.UnitTests;

internal sealed class InMemoryPaymentRepository :
    IRepository<ECommerce.Payment.Domain.Payment, Guid>,
    IUnitOfWork,
    IPaymentIdentityReader
{
    public ECommerce.Payment.Domain.Payment? Payment { get; private set; }

    public int SaveCount { get; private set; }

    public Task<Guid?> FindIdByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Guid? result = Payment?.OrderId == orderId ? Payment.Id : null;
        return Task.FromResult(result);
    }

    public Task<ECommerce.Payment.Domain.Payment?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ECommerce.Payment.Domain.Payment? result = Payment?.Id == id ? Payment : null;
        return Task.FromResult(result);
    }

    public void Add(ECommerce.Payment.Domain.Payment payment)
    {
        Payment = payment;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        SaveCount++;
        return Task.CompletedTask;
    }
}
