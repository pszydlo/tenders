namespace Tenders.Application.Dtos;

public record PagedResultDto<T>(
    IReadOnlyList<T> Items, 
    int PageNumber,
    int PageSize,
    int TotalItems,
    int TotalPages);
