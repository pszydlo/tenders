using FluentValidation;

namespace Tenders.Application.CommandQuery;

public class ValidatorBase<T, TS> : AbstractValidator<T> where T : RequestBase<TS>
{
    protected ValidatorBase()
    {
    }
}
