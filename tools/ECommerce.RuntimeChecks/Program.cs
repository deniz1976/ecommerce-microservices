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
    Environment.ExitCode = 1;
}
catch
{
    Environment.ExitCode = 1;
}
