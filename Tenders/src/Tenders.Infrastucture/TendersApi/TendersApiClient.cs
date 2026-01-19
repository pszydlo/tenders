using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tenders.Infrastucture.TendersApi.Models;
using Tenders.Infrastucture.TendersApi.Options;

namespace Tenders.Infrastucture.TendersApi;

public sealed class TendersApiClient(HttpClient httpClient, ILogger<TendersApiClient> logger)
    : ITendersApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = TendersApiJson.Options;

    public async Task<TendersGuruPagedResponse<TenderListItemApiModel>> GetTendersPageAsync(
        int pageNumber,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);

        var url = $"tenders?page={pageNumber}";
        var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning("TendersGuru GET {Url} failed with {StatusCode}. Body: {Body}", url, (int)response.StatusCode, body);
            response.EnsureSuccessStatusCode();
        }

        var payload = await response.Content.ReadFromJsonAsync<TendersGuruPagedResponse<TenderListItemApiModel>>(JsonOptions, cancellationToken);
        return payload ?? throw new InvalidOperationException("Empty response from tenders.guru");
    }

    public async Task<TenderListItemApiModel?> GetTenderByIdAsync(int id, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(id, 1);

        var url = $"tenders/{id}";
        var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning("TendersGuru GET {Url} failed with {StatusCode}. Body: {Body}", url, (int)response.StatusCode, body);
            response.EnsureSuccessStatusCode();
        }

        return await response.Content.ReadFromJsonAsync<TenderListItemApiModel>(JsonOptions, cancellationToken);
    }
}
