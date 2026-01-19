using Microsoft.AspNetCore.Mvc;
using Tenders.Web.Responses.ErrorHandling;

namespace Tenders.Web.Responses.Results;

public class BadRequest : BadRequestObjectResult
{
    public BadRequest(string content) : base(content)
    {
        Value = new ErrorResponseModel(content);
    }
}