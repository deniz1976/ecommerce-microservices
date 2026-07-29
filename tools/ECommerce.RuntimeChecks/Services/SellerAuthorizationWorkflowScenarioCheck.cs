using ECommerce.RuntimeChecks.Configuration;
using ECommerce.RuntimeChecks.Probes;

namespace ECommerce.RuntimeChecks.Services;

internal sealed class SellerAuthorizationWorkflowScenarioCheck : IWorkflowScenarioCheck
{
    private readonly IAuthorizationBoundaryProbe authorizationBoundaryProbe;

    public SellerAuthorizationWorkflowScenarioCheck(
        IAuthorizationBoundaryProbe authorizationBoundaryProbe)
    {
        this.authorizationBoundaryProbe = authorizationBoundaryProbe;
    }

    public WorkflowScenario Scenario => WorkflowScenario.SellerAuthorization;

    public bool RequiresCustomer => false;

    public async Task RunAsync(Guid customerId, CancellationToken cancellationToken)
    {
        Console.WriteLine("Verifying runtime M2M denial at seller authorization boundaries.");

        await authorizationBoundaryProbe.AssertSellerStoreAccessDeniedAsync(
            cancellationToken);
        await authorizationBoundaryProbe.AssertProductImageUploadDeniedAsync(
            Guid.NewGuid(),
            cancellationToken);

        Console.WriteLine("Seller store and media boundaries rejected the runtime M2M token.");
    }
}
