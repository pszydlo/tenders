using Tenders.Infrastucture.TendersApi.Models;

namespace Tenders.Infrastucture.TendersApi;

public interface ITendersApiClient
{
    Task<TendersGuruPagedResponse<TenderListItemApiModel>> GetTendersPageAsync(int pageNumber, CancellationToken cancellationToken);
    Task<TenderListItemApiModel?> GetTenderByIdAsync(int id, CancellationToken cancellationToken);
}
