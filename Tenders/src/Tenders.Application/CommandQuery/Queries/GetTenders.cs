using FluentValidation;
using MediatR;
using Tenders.Application.Abstractions;
using Tenders.Application.Dtos;
using Tenders.Application.Extensions;
using Tenders.Domain.Models;
using static Tenders.Application.Common.ParseEnum;

namespace Tenders.Application.CommandQuery.Queries;

public class GetTenders
{
    public class Query(
        decimal? MinPriceEur = null,
        decimal? MaxPriceEur = null,
        DateOnly? DateFrom = null,
        DateOnly? DateTo = null,
        int? SupplierId = null,
        int PageSize = 100,
        int PageNumber = 1,
        TenderOrderBy OrderBy = TenderOrderBy.Date,
        OrderDirection OrderDir = OrderDirection.Desc) : RequestBase<PagedResultDto<TenderDto>>
    {
        public decimal? MinPriceEur { get; } = MinPriceEur;
        public decimal? MaxPriceEur { get; } = MaxPriceEur;
        public DateOnly? DateFrom { get; } = DateFrom;
        public DateOnly? DateTo { get; } = DateTo;
        public int? SupplierId { get; } = SupplierId;
        public int PageSize { get; } = PageSize;
        public int PageNumber { get; } = PageNumber;
        public TenderOrderBy OrderBy { get; } = OrderBy;
        public OrderDirection OrderDirection { get; } = OrderDir; 
    }

    public class QueryValidator : ValidatorBase<Query, PagedResultDto<TenderDto>>
    {
        public QueryValidator()
        {
            RuleFor(command => command.PageSize).GreaterThanOrEqualTo(1);
            RuleFor(command => command.PageNumber).GreaterThanOrEqualTo(1);
            RuleFor(command => command.OrderBy)
                .Must(x => x == TenderOrderBy.Date || x == TenderOrderBy.PriceEur)
                .WithMessage("OrderBy must be 'Date' or 'PriceEur'.");
            RuleFor(command => command.OrderDirection)
                .Must(x => x == OrderDirection.Asc || x == OrderDirection.Desc)
                .WithMessage("OrderDirection must be 'Asc' or 'Desc'.");
        }
    }

    public class Handler(ITendersRepository repository) : IRequestHandler<Query, PagedResultDto<TenderDto>>
    {
        public async Task<PagedResultDto<TenderDto>> Handle(Query query, CancellationToken cancellationToken)
        {
             var result = await repository.SearchAsync(
                new TenderSearchCriteria(
                    query.MinPriceEur,
                    query.MaxPriceEur,
                    query.DateFrom,
                    query.DateTo,
                    query.SupplierId,
                    query.PageSize,
                    query.PageNumber,
                    query.OrderBy,
                    query.OrderDirection),
                cancellationToken);
            return result.ToDto(TendersExtensions.ToDto);
        }
    }
}
