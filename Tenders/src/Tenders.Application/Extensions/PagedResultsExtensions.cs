using Tenders.Application.Dtos;
using Tenders.Domain.Models;

namespace Tenders.Application.Extensions;

public static class PagedResultsExtensions
{
    public static PagedResultDto<TDto> ToDto<T, TDto>(
        this PagedResult<T> pagedResult,
        Func<T, TDto> map)
    {
        return new PagedResultDto<TDto>(
            pagedResult.Items.Select(map).ToList(),
            pagedResult.PageNumber,
            pagedResult.PageSize,
            pagedResult.TotalItems,
            pagedResult.TotalPages
        );
    }
}
