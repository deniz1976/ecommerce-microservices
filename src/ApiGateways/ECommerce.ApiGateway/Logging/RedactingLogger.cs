namespace ECommerce.ApiGateway.Logging;

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
