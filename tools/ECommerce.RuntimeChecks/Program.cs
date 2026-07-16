using ECommerce.RuntimeChecks.Configuration;
using ECommerce.RuntimeChecks.Services;

using var cancellationSource = new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationSource.Cancel();
};

try
{
    RuntimeCheckOptions options = RuntimeCheckOptions.Parse(args);
    using HttpClient httpClient = new();
    WorkflowCheckRunner runner = new(httpClient, options);

    await runner.RunAsync(cancellationSource.Token);
}
catch (OperationCanceledException) when (cancellationSource.IsCancellationRequested)
{
    Console.Error.WriteLine("Runtime workflow check was cancelled.");
    Environment.ExitCode = 1;
}
catch (Exception exception)
{
    Console.Error.WriteLine(
        $"Runtime workflow check failed: {exception.GetType().Name}: {exception.Message}");

    if (!string.IsNullOrWhiteSpace(exception.StackTrace))
    {
        Console.Error.WriteLine(exception.StackTrace);
    }

    Environment.ExitCode = 1;
}
