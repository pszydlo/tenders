using System.Text.Json;

namespace Tenders.Infrastucture.TendersApi.Options;

internal static class TendersApiJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };
}
