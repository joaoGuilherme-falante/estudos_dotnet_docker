namespace TaskFlow.Api.Data;

public sealed class DatabaseInitializationHostedService(
    IServiceProvider services,
    IConfiguration configuration,
    ILogger<DatabaseInitializationHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Applying database migrations and initializing bootstrap administrator.");
        await DatabaseInitializer.InitializeAsync(services, configuration);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
