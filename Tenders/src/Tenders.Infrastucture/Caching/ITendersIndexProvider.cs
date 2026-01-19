using Tenders.Domain.Models;

namespace Tenders.Infrastucture.Caching;

public interface ITendersIndexProvider
{
    Task<IReadOnlyList<Tender>> GetIndexAsync(CancellationToken cancellationToken);
    Task<bool> TryWarmUpFromDiskAsync(CancellationToken cancellationToken);
    Task RefreshAsync(CancellationToken cancellationToken);
}
