using ECommerce.RuntimeChecks.Clients;
using ECommerce.RuntimeChecks.Configuration;
using ECommerce.RuntimeChecks.Models;

namespace ECommerce.RuntimeChecks.Services;

internal sealed class WorkflowCheckRunner
{
    private readonly IGatewayWorkflowClient gatewayClient;
    private readonly RuntimeCheckOptions options;
    private readonly IReadOnlyCollection<IWorkflowScenarioCheck> scenarioChecks;

    public WorkflowCheckRunner(
        IGatewayWorkflowClient gatewayClient,
        RuntimeCheckOptions options,
        IReadOnlyCollection<IWorkflowScenarioCheck> scenarioChecks)
    {
        this.gatewayClient = gatewayClient;
        this.options = options;
        this.scenarioChecks = scenarioChecks;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"Starting runtime workflow scenario: {options.Scenario}");

        string suffix = Guid.NewGuid().ToString("N")[..12];
        Console.WriteLine("Registering an isolated workflow-check user.");
        UserResponse user = await gatewayClient.RegisterUserAsync(
            new CreateUserRequest(
                $"workflow-{suffix}@example.com",
                $"Workflow Check {suffix}",
                "WorkflowCheck123!"),
            cancellationToken);
        Console.WriteLine($"Workflow-check user registered: {user.Id}");

        IWorkflowScenarioCheck[] selectedChecks = scenarioChecks
            .Where(check => options.Scenario == WorkflowScenario.All || check.Scenario == options.Scenario)
            .ToArray();
        if (selectedChecks.Length == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options.Scenario), options.Scenario, "Unsupported workflow scenario.");
        }

        foreach (IWorkflowScenarioCheck check in selectedChecks)
        {
            await check.RunAsync(user.Id, cancellationToken);
        }

        Console.WriteLine($"Runtime workflow scenario completed: {options.Scenario}");
    }
}
