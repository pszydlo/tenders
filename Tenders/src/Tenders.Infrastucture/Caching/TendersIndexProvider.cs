using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tenders.Application.Exceptions;
using Tenders.Domain.Models;
using Tenders.Infrastucture.Options;
using Tenders.Infrastucture.Persistence;
using Tenders.Infrastucture.TendersApi;
using Tenders.Infrastucture.TendersApi.Models;

namespace Tenders.Infrastucture.Caching;

public sealed class TendersIndexProvider(
        ITendersApiClient client,
        ITendersPagesStore pagesStore,
        IMemoryCache cache,
        IOptions<TendersOptions> options,
        ILogger<TendersIndexProvider> logger)
    : ITendersIndexProvider
{
    private const string CacheKey = "tenders.guru.pl:pl:tenders:index:first100pages:v1";

    private readonly MemoryCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(options.Value.RefreshDataInHours)
    };

    private static readonly SemaphoreSlim BuildLock = new(1, 1);
    private const int MaxConcurrency = 2;

    public Task<IReadOnlyList<Tender>> GetIndexAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue<IReadOnlyList<Tender>>(CacheKey, out var cached) && cached is not null)
            return Task.FromResult(cached);

        throw new ServiceUnavailableException("Tender index is being built. Please retry shortly.",
        retryAfterSeconds: 30);
    }

    public async Task RefreshAsync(CancellationToken cancellationToken)
    {
        await BuildLock.WaitAsync(cancellationToken);
        try
        {
            logger.LogInformation("Refreshing tenders index (1..{MaxPages})", GetMaxPages());

            var built = await BuildIndexAsync(cancellationToken);
            cache.Set(CacheKey, built, CacheOptions);

            logger.LogInformation("Tenders index refreshed. Items: {Count}", built.Count);
        }
        catch (OperationCanceledException)
        {
            // ok - shutdown / cancel
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Refreshing tenders index failed. Keeping stale cache.");
        }
        finally
        {
            BuildLock.Release();
        }
    }

    private int GetMaxPages()
        => Math.Max(1, Math.Min(options.Value.MaxSourcePages, 100));

    private async Task<IReadOnlyList<Tender>> BuildIndexAsync(CancellationToken cancellationToken)
    {
        var maxPages = GetMaxPages();

        var tenders = new ConcurrentBag<Tender>();
        var failedPages = new ConcurrentQueue<int>();

        await FetchPagesAsync(
            pages: Enumerable.Range(1, maxPages),
            onSuccess: (page, pageResponse, _) =>
            {
                foreach (var item in pageResponse.Data)
                    tenders.Add(ToDomain(item));
                logger.LogInformation("Refresh page {Page}: OK ", page);
                return Task.CompletedTask;
            },
            onFailure: (page, _) =>
            {
                failedPages.Enqueue(page);
                logger.LogInformation("Refresh page {Page}: FAILED ", page);
                return Task.CompletedTask;
            },
            cancellationToken);

        if (!failedPages.IsEmpty)
        {
            var pagesToRetry = failedPages.Distinct().ToArray();
            logger.LogWarning(
                "Initial fetch failed for {Count} pages. Retrying once...",
                pagesToRetry.Length);

            await Task.Delay(TimeSpan.FromMilliseconds(300), cancellationToken);

            var stillFailed = new ConcurrentQueue<int>();

            await FetchPagesAsync(
                pages: pagesToRetry,
                onSuccess: (page, pageResponse, _) =>
                {
                    foreach (var item in pageResponse.Data)
                        tenders.Add(ToDomain(item));

                    logger.LogInformation("Refresh page {Page}: OK ", page);
                    return Task.CompletedTask;
                },
                onFailure: (page, _) =>
                {
                    stillFailed.Enqueue(page);
                    logger.LogInformation("Refresh page {Page}: FAILED ", page);
                    return Task.CompletedTask;
                },
                cancellationToken);

            if (!stillFailed.IsEmpty)
            {
                var failed = stillFailed.Distinct().ToArray();
                logger.LogWarning(
                    "Retry still failed for {Count} pages: {Pages}",
                    failed.Length,
                    string.Join(",", failed));
            }
        }

        return tenders.ToList();
    }

    private async Task FetchPagesAsync(
        IEnumerable<int> pages,
        Func<int, TendersGuruPagedResponse<TenderListItemApiModel>, CancellationToken, Task> onSuccess,
        Func<int, CancellationToken, Task> onFailure,
        CancellationToken cancellationToken)
    {
        await Parallel.ForEachAsync(
            pages,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxConcurrency,
                CancellationToken = cancellationToken
            },
            async (page, ct) =>
            {
                try
                {
                    var pageResponse = await client.GetTendersPageAsync(page, ct);
                    await pagesStore.WriteAsync(page, pageResponse, ct);
                    await onSuccess(page, pageResponse, ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    // shutdown/cancel
                }
                catch (Exception)
                {
                    logger.LogWarning("Failed to fetch tenders page {Page} from API. Trying disk cache.", page);

                    var stored = await pagesStore.TryReadAsync(page, ct);
                    if (stored is not null)
                    {
                        logger.LogInformation("Using cached tenders page {Page} from disk.", page);
                        await onSuccess(page, stored, ct);
                        return;
                    }

                    await onFailure(page, ct);
                }
            });
    }

    public async Task<bool> TryWarmUpFromDiskAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue<IReadOnlyList<Tender>>(CacheKey, out var cached) && cached is not null)
            return true;

        await BuildLock.WaitAsync(cancellationToken);
        try
        {
            if (cache.TryGetValue<IReadOnlyList<Tender>>(CacheKey, out cached) && cached is not null)
                return true;

            var maxPages = GetMaxPages();
            var loaded = new List<Tender>();
            var pagesLoaded = 0;

            for (var page = 1; page <= maxPages; page++)
            {
                var stored = await pagesStore.TryReadAsync(page, cancellationToken);
                if (stored is null)
                    continue;

                pagesLoaded++;
                foreach (var item in stored.Data)
                    loaded.Add(ToDomain(item));
            }

            if (pagesLoaded == 0)
            {
                logger.LogInformation("No cached pages found on disk. Index will be built from API.");
                return false;
            }

            cache.Set(CacheKey, loaded, CacheOptions);
            logger.LogInformation("Warmed up tenders index from disk. Pages: {Pages}, Items: {Items}", pagesLoaded, loaded.Count);
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to warm up tenders index from disk.");
            return false;
        }
        finally
        {
            BuildLock.Release();
        }
    }

    private static Tender ToDomain(TenderListItemApiModel api)
    {
        var amountEur = api.AwardedValueEur ?? 0m;

        var suppliers =
            api.Awarded?
                .SelectMany(a => a.Suppliers ?? [])
                .Where(s => !string.IsNullOrWhiteSpace(s.Name))
                .GroupBy(s => s.Id)
                .Select(g => new Supplier(g.Key, g.First().Name!))
                .ToList()
            ?? [];

        return new Tender(
            api.Id,
            api.Date,
            api.Title ?? string.Empty,
            api.Description ?? string.Empty,
            amountEur,
            suppliers
        );
    }
}
