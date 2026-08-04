using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Payment.Application.Commands.AuthorizePayment;
using ECommerce.Payment.Application.Commands.RefundPayment;
using ECommerce.Payment.Application.Payments;
using ECommerce.Payment.Application.Queries.GetPaymentByOrderId;
using ECommerce.Payment.Application.Queries.SearchManagedPayments;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Payment.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddScoped<PaymentService>();
        services.AddScoped<PaymentQueryService>();
        services.AddScoped<
            ICommandHandler<AuthorizePaymentCommand, PaymentAuthorizationResult>,
            AuthorizePaymentCommandHandler>();
        services.AddScoped<ICommandHandler<RefundPaymentCommand>, RefundPaymentCommandHandler>();
        services.AddScoped<
            IQueryHandler<GetPaymentByOrderIdQuery, Result<PaymentResponse>>,
            GetPaymentByOrderIdQueryHandler>();
        services.AddScoped<
            IQueryHandler<SearchManagedPaymentsQuery, PagedResult<PaymentSummaryResponse>>,
            SearchManagedPaymentsQueryHandler>();
        return services;
    }
}
