using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Persistence;
using ECommerce.Payment.Application.Payments;
using ECommerce.Payment.Infrastructure.Payments;
using ECommerce.Payment.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Payment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresDbContext<PaymentDbContext>(configuration, "PaymentDb");
        services.AddOptions<DemoPaymentOptions>()
            .Bind(configuration.GetSection(DemoPaymentOptions.SectionName))
            .Validate(
                options => Enum.IsDefined(options.Scenario),
                "DemoPayment:Scenario must be a supported demo payment scenario.")
            .Validate(
                options => options.AuthorizationDelayMilliseconds is >= 0 and <= 30_000,
                "DemoPayment:AuthorizationDelayMilliseconds must be between 0 and 30000.")
            .ValidateOnStart();
        services.AddScoped<IPaymentIdentityReader, PaymentIdentityReader>();
        services.AddScoped<IPaymentQueryReader, PaymentQueryReader>();
        services.AddScoped<IRepository<Domain.Payment, Guid>>(serviceProvider =>
            new EfRepository<Domain.Payment, Guid>(
                serviceProvider.GetRequiredService<PaymentDbContext>(),
                payment => payment.Id));
        services.AddScoped<IUnitOfWork, EfUnitOfWork<PaymentDbContext>>();
        services.AddScoped<IPaymentProvider, DemoPaymentProvider>();
        return services;
    }
}
