using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tenders.Infrastucture.Caching;
using Tenders.Infrastucture.Options;

namespace Tenders.Infrastucture.Background;

public sealed class TendersIndexRefreshHostedService(
    ITendersIndexProvider indexProvider,
    IOptions<TendersOptions> options,
    ILogger<TendersIndexRefreshHostedService> logger)
    : BackgroundService
{
    private readonly TimeSpan RefreshInterval = TimeSpan.FromHours(options.Value.RefreshDataInHours);
    private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(InitialDelay, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await indexProvider.RefreshAsync(stoppingToken);
                await Task.Delay(RefreshInterval, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("TendersIndexRefreshHostedService stopped.");
        }
    }
}
