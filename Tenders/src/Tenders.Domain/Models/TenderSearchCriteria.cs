namespace Tenders.Domain.Models;

public record TenderSearchCriteria(
    decimal? MinPriceEur,
    decimal? MaxPriceEur,
    DateOnly? DateFrom,
    DateOnly? DateTo,
    int? SupplierId,
    int PageSize,
    int PageNumber,
    TenderOrderBy OrderBy,
    OrderDirection OrderDirection);
