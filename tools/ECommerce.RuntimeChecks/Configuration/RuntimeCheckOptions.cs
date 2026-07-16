namespace ECommerce.RuntimeChecks.Configuration;

public sealed record RuntimeCheckOptions(
    Uri GatewayBaseUri,
    TimeSpan Timeout,
    TimeSpan PollingInterval,
    string AccessToken,
    WorkflowScenario Scenario)
{
    public static RuntimeCheckOptions Parse(string[] args)
    {
        string gateway = ReadArgument(args, "--gateway") ?? "http://localhost:5080";
        string timeoutSeconds = ReadArgument(args, "--timeout-seconds") ?? "60";
        WorkflowScenario scenario = WorkflowScenarios.Parse(ReadArgument(args, "--scenario"));
        string accessToken = Environment.GetEnvironmentVariable("RuntimeChecks__AccessToken") ?? string.Empty;

        if (!Uri.TryCreate(gateway, UriKind.Absolute, out Uri? gatewayUri) ||
            (gatewayUri.Scheme != Uri.UriSchemeHttp && gatewayUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("--gateway must be an absolute HTTP or HTTPS URL.");
        }

        if (!int.TryParse(timeoutSeconds, out int seconds) || seconds <= 0)
        {
            throw new ArgumentException("--timeout-seconds must be a positive integer.");
        }

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException(
                "RuntimeChecks__AccessToken must contain an Auth0 access token authorized for the runtime workflow.");
        }

        return new RuntimeCheckOptions(
            new Uri(gatewayUri.GetLeftPart(UriPartial.Authority)),
            TimeSpan.FromSeconds(seconds),
            TimeSpan.FromSeconds(2),
            accessToken.Trim(),
            scenario);
    }

    private static string? ReadArgument(IReadOnlyList<string> args, string name)
    {
        for (int index = 0; index < args.Count - 1; index++)
        {
            if (string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase))
            {
                return args[index + 1];
            }
        }

        return null;
    }
}
