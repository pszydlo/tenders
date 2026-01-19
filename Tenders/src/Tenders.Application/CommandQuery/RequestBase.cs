using MediatR;

namespace Tenders.Application.CommandQuery;

public class RequestBase<T> : IRequest<T>
{
}