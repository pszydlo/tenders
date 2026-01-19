using Tenders.Infrastucture.TendersApi.Models;

namespace Tenders.Infrastucture.Persistence;

public interface ITendersPagesStore
{
    Task<TendersGuruPagedResponse<TenderListItemApiModel>?> TryReadAsync(int pageNumber, CancellationToken cancellationToken);
    Task WriteAsync(int pageNumber, TendersGuruPagedResponse<TenderListItemApiModel> page, CancellationToken cancellationToken);
}
