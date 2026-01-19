using FluentValidation;
using MediatR;
using Tenders.Application.Abstractions;
using Tenders.Application.Dtos;
using Tenders.Application.Exceptions;
using Tenders.Application.Extensions;

namespace Tenders.Application.CommandQuery.Queries;

public class GetTenderById
{
    public class Query(int Id) : RequestBase<TenderDto>
    {
        public int Id { get; } = Id;
    }

    public class QueryValidator : ValidatorBase<Query, TenderDto>
    {
        public QueryValidator()
        {
            RuleFor(command => command.Id).GreaterThanOrEqualTo(1);
        }
    }

    public class Handler(ITendersRepository repository) : IRequestHandler<Query, TenderDto>
    {
        public async Task<TenderDto> Handle(Query query, CancellationToken cancellationToken)
        {
            var result = await repository.GetByIdAsync(query.Id, cancellationToken);
            return result == null ?
                throw new NotFoundException() : TendersExtensions.ToDto(result);
        }
    }
}
