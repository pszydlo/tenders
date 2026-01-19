using MediatR;

namespace Tenders.Application.CommandQuery;

public class RequestHandler(IMediator mediator)
{
    public async Task<TResponse> ProcessRequest<TResponse>(RequestBase<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        return await mediator.Send(request, cancellationToken);
    }
}