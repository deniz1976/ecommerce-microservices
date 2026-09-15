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

internal sealed class RedactingLogger(ILogger inner) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return inner.BeginScope(state);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return inner.IsEnabled(logLevel);
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (state is IReadOnlyList<KeyValuePair<string, object?>> values)
        {
            RedactedLogState redactedState = new(values, formatter(state, exception));
            inner.Log(
                logLevel,
                eventId,
                redactedState,
                exception,
                static (currentState, _) => currentState.ToString());
            return;
        }

        inner.Log(
            logLevel,
            eventId,
            state,
            exception,
            (currentState, currentException) =>
                SensitiveQueryRedactor.Redact(formatter(currentState, currentException)));
    }
}

internal sealed class RedactedLogState : IReadOnlyList<KeyValuePair<string, object?>>
{
    private readonly KeyValuePair<string, object?>[] values;
    private readonly string message;

    public RedactedLogState(IReadOnlyList<KeyValuePair<string, object?>> source, string message)
    {
        values = new KeyValuePair<string, object?>[source.Count];
        for (int index = 0; index < source.Count; index++)
        {
            KeyValuePair<string, object?> entry = source[index];
            values[index] = new KeyValuePair<string, object?>(
                entry.Key,
                SensitiveQueryRedactor.RedactValue(entry.Value));
        }

        this.message = SensitiveQueryRedactor.Redact(message);
    }

    public KeyValuePair<string, object?> this[int index] => values[index];

    public int Count => values.Length;

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        return ((IEnumerable<KeyValuePair<string, object?>>)values).GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override string ToString()
    {
        return message;
    }
}
