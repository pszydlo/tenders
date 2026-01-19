using Microsoft.AspNetCore.Mvc;
using Tenders.Web.Responses.ErrorHandling;

namespace Tenders.Web.Responses.Results;

public class InternalServerError : ObjectResult
{
    private const int DefaultStatusCode = StatusCodes.Status500InternalServerError;

    public InternalServerError(string content): base(content)
    {
        StatusCode = DefaultStatusCode;
        Value = new ErrorResponseModel(content);
    }
}