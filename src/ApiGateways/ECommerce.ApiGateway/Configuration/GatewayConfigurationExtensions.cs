namespace ECommerce.ApiGateway.Configuration;

public static class GatewayConfigurationExtensions
{
    public static IConfigurationBuilder AddGatewayRoutes(
        this IConfigurationBuilder configuration,
        IHostEnvironment environment)
    {
        string environmentPath = Path.Combine(
            AppContext.BaseDirectory,
            $"ocelot.{environment.EnvironmentName}.json");
        string path = File.Exists(environmentPath)
            ? environmentPath
            : Path.Combine(AppContext.BaseDirectory, "ocelot.json");

        return configuration.AddJsonFile(path, optional: false, reloadOnChange: true);
    }
}
