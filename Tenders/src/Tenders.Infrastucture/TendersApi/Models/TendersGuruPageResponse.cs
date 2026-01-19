using System.Text.Json.Serialization;

namespace Tenders.Infrastucture.TendersApi.Models;

public sealed record TendersGuruPagedResponse<T>(
    [property: JsonPropertyName("page_count")] int PageCount,
    [property: JsonPropertyName("page_number")] int PageNumber,
    [property: JsonPropertyName("page_size")] int PageSize,
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("data")] IReadOnlyList<T> Data
);
