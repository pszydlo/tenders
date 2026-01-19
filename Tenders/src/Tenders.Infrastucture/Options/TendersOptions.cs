namespace Tenders.Infrastucture.Options;

public sealed class TendersOptions
{
    public const string SectionName = "Tenders";
    public required string BaseUrl { get; set; }

    public int MaxSourcePages { get; set; } = 100;

    public int RefreshDataInHours { get; set; } = 24;
    public bool PersistPagesToDisk { get; set; } = true;
    public string PagesCacheDirectory { get; set; } = "App_Data/tenders-pages";
}
