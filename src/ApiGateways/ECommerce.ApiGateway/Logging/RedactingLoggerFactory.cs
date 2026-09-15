using Microsoft.Extensions.Options;

namespace ECommerce.ApiGateway.Logging;

public sealed class RedactingLoggerFactory : ILoggerFactory
{
    private readonly ILoggerFactory inner;

    public RedactingLoggerFactory(
        IEnumerable<ILoggerProvider> providers,
        IOptionsMonitor<LoggerFilterOptions> filterOptions)
    {
        inner = new LoggerFactory(providers, filterOptions);
    }

    public void AddProvider(ILoggerProvider provider)
    {
        inner.AddProvider(provider);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new RedactingLogger(inner.CreateLogger(categoryName));
    }

    public void Dispose()
    {
        inner.Dispose();
    }
}
