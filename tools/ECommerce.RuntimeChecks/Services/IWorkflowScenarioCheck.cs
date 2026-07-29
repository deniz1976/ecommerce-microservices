using ECommerce.RuntimeChecks.Configuration;

namespace ECommerce.RuntimeChecks.Services;

internal interface IWorkflowScenarioCheck
{
    WorkflowScenario Scenario { get; }

    bool IncludeInAll => true;

    bool RequiresCustomer => true;

    Task RunAsync(Guid customerId, CancellationToken cancellationToken);
}
