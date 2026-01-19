using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tenders.Infrastucture.Caching;

namespace Tenders.Infrastucture.Background;

public sealed class TendersIndexBootstrapHostedService(
    ITendersIndexProvider indexProvider,
    ILogger<TendersIndexBootstrapHostedService> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var warmed = await indexProvider.TryWarmUpFromDiskAsync(cancellationToken);
            if (!warmed)
                logger.LogInformation("Tenders index not warmed from disk (no cache yet).");
        }
        catch (OperationCanceledException)
        {
            // shutdown
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
