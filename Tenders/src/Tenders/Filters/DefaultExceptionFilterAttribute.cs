using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Tenders.Application.Exceptions;
using Tenders.Web.Responses.Results;

namespace Tenders.Web.Filters;

public class DefaultExceptionFilterAttribute(ILogger<DefaultExceptionFilterAttribute> logger) : ExceptionFilterAttribute
{
    public override async Task OnExceptionAsync(ExceptionContext context)
    {
        await base.OnExceptionAsync(context);

        var exception = context.Exception;

        if (exception is ValidationException or NotFoundException or ServiceUnavailableException)
            logger.LogWarning(exception, "Request failed with {ExceptionType}", exception.GetType().Name);
        else
            logger.LogError(exception, "Unhandled exception: {ExceptionType}", exception.GetType().Name);

        context.Result = exception switch
        {
            ValidationException e => new ValidationError(exception.Message, e),
            NotFoundException => new NotFoundResult(),
            ServiceUnavailableException e => new ServiceUnavailable(e.Message, e.RetryAfterSeconds),
            _ => new InternalServerError($"{exception.Message} | Inner Message: {exception.InnerException?.Message}")
        };

        if (exception is ServiceUnavailableException sue && sue.RetryAfterSeconds is not null)
            context.HttpContext.Response.Headers.RetryAfter = sue.RetryAfterSeconds.Value.ToString();

        context.ExceptionHandled = true;
    }
}
