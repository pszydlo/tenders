using System.Text.Json.Serialization;

namespace Tenders.Infrastucture.TendersApi.Models;

public sealed record AwardedSuppliersApiModel(
    [property: JsonPropertyName("suppliers")] IReadOnlyList<SupplierApiModel>? Suppliers
);

public sealed record SupplierApiModel(
    [property: JsonPropertyName("id")]
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    int Id,
    [property: JsonPropertyName("name")] string? Name
);
