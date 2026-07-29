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

        IWorkflowScenarioCheck[] selectedChecks = scenarioChecks
            .Where(check =>
                options.Scenario == WorkflowScenario.All
                    ? check.IncludeInAll
                    : check.Scenario == options.Scenario)
            .ToArray();
        if (selectedChecks.Length == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options.Scenario), options.Scenario, "Unsupported workflow scenario.");
        }

        Guid customerId = Guid.Empty;
        if (selectedChecks.Any(check => check.RequiresCustomer))
        {
            string suffix = Guid.NewGuid().ToString("N")[..12];
            Console.WriteLine("Registering an isolated workflow-check user.");
            UserResponse user = await gatewayClient.RegisterUserAsync(
                new CreateUserRequest(
                    $"workflow-{suffix}@example.com",
                    $"Workflow Check {suffix}",
                    "WorkflowCheck123!"),
                cancellationToken);
            customerId = user.Id;
            Console.WriteLine($"Workflow-check user registered: {customerId}");
        }

        foreach (IWorkflowScenarioCheck check in selectedChecks)
        {
            await check.RunAsync(customerId, cancellationToken);
        }

        Console.WriteLine($"Runtime workflow scenario completed: {options.Scenario}");
    }
}
