using Tenders.Application.Abstractions;
using Tenders.Domain.Models;
using Tenders.Infrastucture.Caching;

namespace Tenders.Infrastucture.Repositories;

public sealed class TendersRepository(ITendersIndexProvider indexProvider)
    : ITendersRepository
{
    public async Task<PagedResult<Tender>> SearchAsync(
        TenderSearchCriteria criteria,
        CancellationToken cancellationToken)
    {
        var index = await indexProvider.GetIndexAsync(cancellationToken);

        IEnumerable<Tender> query = index;

        if (criteria.MinPriceEur is not null)
            query = query.Where(t => t.Amount >= criteria.MinPriceEur.Value);

        if (criteria.MaxPriceEur is not null)
            query = query.Where(t => t.Amount <= criteria.MaxPriceEur.Value);

        if (criteria.DateFrom is not null)
            query = query.Where(t => t.Date >= criteria.DateFrom.Value);

        if (criteria.DateTo is not null)
            query = query.Where(t => t.Date <= criteria.DateTo.Value);

        if (criteria.SupplierId is not null)
            query = query.Where(t => t.Suppliers.Any(s => s.Id == criteria.SupplierId.Value));

        query = (criteria.OrderBy, criteria.OrderDirection) switch
        {
            (TenderOrderBy.PriceEur, OrderDirection.Asc) => query.OrderBy(t => t.Amount).ThenBy(t => t.Id),
            (TenderOrderBy.PriceEur, OrderDirection.Desc) => query.OrderByDescending(t => t.Amount).ThenByDescending(t => t.Id),
            (TenderOrderBy.Date, OrderDirection.Asc) => query.OrderBy(t => t.Date).ThenBy(t => t.Id),
            _ => query.OrderByDescending(t => t.Date).ThenByDescending(t => t.Id)
        };

        var total = query.Count();

        var items = query
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToList();

        return new PagedResult<Tender>(items, criteria.PageNumber, criteria.PageSize, total, (int)Math.Ceiling(total / (double)criteria.PageSize));
    }

    public async Task<Tender?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var index = await indexProvider.GetIndexAsync(cancellationToken);
        return index.FirstOrDefault(t => t.Id == id);
    }
}
