using System.Diagnostics;
using ECommerce.BuildingBlocks.Observability;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace ECommerce.ContractTests;

public sealed class ObservabilityRegistrationTests
{
    [Fact]
    public void AddECommerceObservabilityRegistersTraceMetricAndLogProviders()
    {
        ServiceCollection services = new();
        IConfiguration configuration = new ConfigurationBuilder().Build();
        services.AddLogging();

        services.AddECommerceObservability(configuration, "ECommerce.TestService");

        using ServiceProvider provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<TracerProvider>());
        Assert.NotNull(provider.GetService<MeterProvider>());
        Assert.Contains(
            provider.GetServices<ILoggerProvider>(),
            loggerProvider => loggerProvider.GetType().Name == "OpenTelemetryLoggerProvider");
    }

    [Fact]
    public void AddECommerceObservabilityAcceptsStandardOtlpConfiguration()
    {
        Dictionary<string, string?> settings = new()
        {
            ["OTEL_EXPORTER_OTLP_ENDPOINT"] = "https://otlp.example.test/otlp",
            ["OTEL_EXPORTER_OTLP_PROTOCOL"] = "http/protobuf",
            ["OTEL_EXPORTER_OTLP_HEADERS"] = "Authorization=Basic%20masked"
        };
        ServiceCollection services = new();
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
        services.AddLogging();

        services.AddECommerceObservability(configuration, "ECommerce.TestService");

        using ServiceProvider provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<TracerProvider>());
        Assert.NotNull(provider.GetService<MeterProvider>());
        Assert.Contains(
            provider.GetServices<ILoggerProvider>(),
            loggerProvider => loggerProvider.GetType().Name == "OpenTelemetryLoggerProvider");
    }

    [Fact]
    public void SensitiveActivityProcessorRedactsSensitiveTagsAndPreservesSafeTags()
    {
        using Activity activity = new("redaction-test");
        activity.SetTag("http.request.method", "GET");
        activity.SetTag("http.request.header.authorization", "Bearer top-secret");
        activity.SetTag("url.query", "token=top-secret");
        activity.Start();

        SensitiveActivityProcessor processor = new();
        processor.OnEnd(activity);

        Assert.Equal("GET", activity.GetTagItem("http.request.method"));
        Assert.Equal(SensitiveDataRedaction.RedactedValue, activity.GetTagItem("http.request.header.authorization"));
        Assert.Equal(SensitiveDataRedaction.RedactedValue, activity.GetTagItem("url.query"));
    }

    [Fact]
    public void RedactedLogAttributesRedactsSensitiveFieldsAndPreservesSafeFields()
    {
        IReadOnlyList<KeyValuePair<string, object?>> attributes =
        [
            new("OrderId", Guid.Parse("11111111-1111-1111-1111-111111111111")),
            new("Password", "top-secret"),
            new("connection_string", string.Join(';', "Host=example", "Password=top-secret"))
        ];

        RedactedLogAttributes redacted = new(attributes);

        Assert.Equal(attributes[0], redacted[0]);
        Assert.Equal(SensitiveDataRedaction.RedactedValue, redacted[1].Value);
        Assert.Equal(SensitiveDataRedaction.RedactedValue, redacted[2].Value);
    }

    [Fact]
    public void SensitiveLogTextRedactionRemovesBearerTokensAndCredentialValues()
    {
        string source = "Request failed Authorization=Bearer abc.def Password=top-secret";

        string? redacted = SensitiveLogTextRedaction.Redact(source);

        Assert.DoesNotContain("abc.def", redacted);
        Assert.DoesNotContain("top-secret", redacted);
        Assert.Contains(SensitiveDataRedaction.RedactedValue, redacted);
    }
}
