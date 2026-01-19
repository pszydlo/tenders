using Tenders.Domain.Models;

namespace Tenders.Application.Abstractions;

public interface ITendersRepository
{
    Task<PagedResult<Tender>> SearchAsync(
    TenderSearchCriteria criteria,
    CancellationToken cancellationToken);

    Task<Tender?> GetByIdAsync(int id, CancellationToken cancellationToken);
}
