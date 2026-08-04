using ECommerce.RuntimeChecks.Configuration;
using ECommerce.RuntimeChecks.Clients;
using ECommerce.RuntimeChecks.Probes;
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
    IGatewayWorkflowClient gatewayClient = new GatewayWorkflowClient(httpClient, options.GatewayBaseUri, options.AccessToken);
    IAuthorizationBoundaryProbe authorizationBoundaryProbe = new GatewayAuthorizationBoundaryProbe(
        httpClient,
        options.GatewayBaseUri,
        options.AccessToken);
    IWorkflowProbe workflowProbe = new PostgresWorkflowProbe();
    ICatalogProductFixture catalogProductFixture = new PostgresCatalogProductFixture();
    INotificationLiveDeliveryProbe notificationLiveDeliveryProbe = new SignalRNotificationLiveDeliveryProbe(options);
    WorkflowScenarioContext scenarioContext = new(gatewayClient, workflowProbe, options);
    IWorkflowScenarioCheck[] scenarioChecks =
    [
        new BasketCheckoutWorkflowScenarioCheck(scenarioContext, catalogProductFixture),
        new SuccessWorkflowScenarioCheck(scenarioContext),
        new InventoryFailureWorkflowScenarioCheck(scenarioContext),
        new PaymentFailureWorkflowScenarioCheck(scenarioContext),
        new PaymentDeclineWorkflowScenarioCheck(scenarioContext),
        new NotificationSignalRWorkflowScenarioCheck(scenarioContext, notificationLiveDeliveryProbe),
        new SellerAuthorizationWorkflowScenarioCheck(authorizationBoundaryProbe),
        new ShippingFailureWorkflowScenarioCheck(scenarioContext),
        new CustomerCancellationWorkflowScenarioCheck(scenarioContext)
    ];
    WorkflowCheckRunner runner = new(gatewayClient, options, scenarioChecks);

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
