using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ECommerce.BuildingBlocks.Observability;

public static class DependencyInjection
{
    public static IServiceCollection AddECommerceObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        ObservabilityOptions options = configuration.GetSection(ObservabilityOptions.SectionName).Get<ObservabilityOptions>() ?? new ObservabilityOptions();
        bool useStandardOtlpConfiguration = !string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["service.namespace"] = options.ServiceNamespace
                }))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource(serviceName)
                    .AddSource("MassTransit");

                if (options.RedactionEnabled)
                {
                    tracing.AddProcessor(new SensitiveActivityProcessor());
                }

                if (useStandardOtlpConfiguration)
                {
                    tracing.AddOtlpExporter();
                }
                else if (!string.IsNullOrWhiteSpace(options.OtlpEndpoint))
                {
                    tracing.AddOtlpExporter(exporter =>
                    {
                        exporter.Endpoint = new Uri(options.OtlpEndpoint);
                    });
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(serviceName);

                if (useStandardOtlpConfiguration)
                {
                    metrics.AddOtlpExporter();
                }
                else if (!string.IsNullOrWhiteSpace(options.OtlpEndpoint))
                {
                    metrics.AddOtlpExporter(exporter =>
                    {
                        exporter.Endpoint = new Uri(options.OtlpEndpoint);
                    });
                }
            })
            .WithLogging(logging =>
            {
                if (options.RedactionEnabled)
                {
                    logging.AddProcessor(new SensitiveLogRecordProcessor());
                }

                if (useStandardOtlpConfiguration)
                {
                    logging.AddOtlpExporter();
                }
                else if (!string.IsNullOrWhiteSpace(options.OtlpEndpoint))
                {
                    logging.AddOtlpExporter(exporter =>
                    {
                        exporter.Endpoint = new Uri(options.OtlpEndpoint);
                    });
                }
            }, loggingOptions =>
            {
                loggingOptions.IncludeFormattedMessage = true;
                loggingOptions.IncludeScopes = true;
                loggingOptions.ParseStateValues = true;
            });

        return services;
    }
}
