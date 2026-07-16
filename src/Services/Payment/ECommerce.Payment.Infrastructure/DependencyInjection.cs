using ECommerce.BuildingBlocks.Persistence;
using ECommerce.Payment.Application.Payments;
using ECommerce.Payment.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Payment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresDbContext<PaymentDbContext>(configuration, "PaymentDb");
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        return services;
    }
}
