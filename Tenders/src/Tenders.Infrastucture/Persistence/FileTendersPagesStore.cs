using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tenders.Infrastucture.Options;
using Tenders.Infrastucture.TendersApi;
using Tenders.Infrastucture.TendersApi.Models;
using Tenders.Infrastucture.TendersApi.Options;

namespace Tenders.Infrastucture.Persistence;

public sealed class FileTendersPagesStore(
    IOptions<TendersOptions> options,
    IHostEnvironment hostEnvironment,
    ILogger<FileTendersPagesStore> logger)
    : ITendersPagesStore
{
    private static readonly JsonSerializerOptions JsonOptions = TendersApiJson.Options;

    public async Task<TendersGuruPagedResponse<TenderListItemApiModel>?> TryReadAsync(
        int pageNumber,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);

        if (!options.Value.PersistPagesToDisk)
            return null;

        var path = GetPagePath(pageNumber);
        if (!File.Exists(path))
            return null;

        try
        {
            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<TendersGuruPagedResponse<TenderListItemApiModel>>(
                stream,
                JsonOptions,
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Failed to read cached tenders page {Page} from {Path}.", pageNumber, path);
            return null;
        }
    }

    public async Task WriteAsync(
        int pageNumber,
        TendersGuruPagedResponse<TenderListItemApiModel> page,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentNullException.ThrowIfNull(page);

        if (!options.Value.PersistPagesToDisk)
            return;

        var directory = GetPagesDirectory();
        Directory.CreateDirectory(directory);

        var targetPath = GetPagePath(pageNumber);
        var tempPath = targetPath + ".tmp";

        try
        {
            await using (var stream = File.Create(tempPath))
            {
                await JsonSerializer.SerializeAsync(stream, page, JsonOptions, cancellationToken);
            }

            // Atomic-ish replace on the same volume.
            File.Move(tempPath, targetPath, overwrite: true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Failed to write cached tenders page {Page} to {Path}.", pageNumber, targetPath);
            TryDelete(tempPath);
        }
    }

    private string GetPagesDirectory()
        => Path.Combine(hostEnvironment.ContentRootPath, options.Value.PagesCacheDirectory);

    private string GetPagePath(int pageNumber)
        => Path.Combine(GetPagesDirectory(), $"tenders-page-{pageNumber:000}.json");

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // ignore cleanup failures
        }
    }
}
