using Microsoft.AspNetCore.Mvc;

namespace Tenders.Web.Responses.Results;

public sealed class ServiceUnavailable : ObjectResult
{
    private const int StatusCodeValue = StatusCodes.Status503ServiceUnavailable;

    public ServiceUnavailable(string message, int? retryAfterSeconds)
        : base(new ServiceUnavailableResponseModel(message, retryAfterSeconds))
    {
        StatusCode = StatusCodeValue;
    }
}

public record ServiceUnavailableResponseModel(
    string Message,
    int? RetryAfterSeconds
);
