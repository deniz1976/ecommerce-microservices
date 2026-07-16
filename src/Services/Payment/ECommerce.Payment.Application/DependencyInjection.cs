using ECommerce.Payment.Application.Payments;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Payment.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentApplication(this IServiceCollection services)
    {
        services.AddScoped<PaymentService>();
        return services;
    }
}
