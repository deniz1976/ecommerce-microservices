using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Payment.Application.Commands.AuthorizePayment;
using ECommerce.Payment.Application.Commands.RefundPayment;
using ECommerce.Payment.Application.Payments;
using ECommerce.Payment.Application.Queries.GetPaymentByOrderId;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Payment.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentApplication(this IServiceCollection services)
    {
        services.AddScoped<
            ICommandHandler<AuthorizePaymentCommand, PaymentAuthorizationResult>,
            AuthorizePaymentCommandHandler>();
        services.AddScoped<ICommandHandler<RefundPaymentCommand>, RefundPaymentCommandHandler>();
        services.AddScoped<
            IQueryHandler<GetPaymentByOrderIdQuery, Result<PaymentResponse>>,
            GetPaymentByOrderIdQueryHandler>();
        return services;
    }
}
