using ECommerce.RuntimeChecks.Configuration;

namespace ECommerce.RuntimeChecks.Services;

internal interface IWorkflowScenarioCheck
{
    WorkflowScenario Scenario { get; }

    Task RunAsync(Guid customerId, CancellationToken cancellationToken);
}
