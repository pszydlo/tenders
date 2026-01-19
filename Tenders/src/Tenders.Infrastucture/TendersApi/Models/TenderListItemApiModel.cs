using System.Text.Json.Serialization;

namespace Tenders.Infrastucture.TendersApi.Models;

public sealed record TenderListItemApiModel(
    [property: JsonPropertyName("id")]
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] int Id,
    [property: JsonPropertyName("date")] DateOnly Date,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("awarded_value_eur")]
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    decimal? AwardedValueEur,
    [property: JsonPropertyName("awarded")] IReadOnlyList<AwardedSuppliersApiModel>? Awarded
);
