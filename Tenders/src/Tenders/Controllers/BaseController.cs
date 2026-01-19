using Microsoft.AspNetCore.Mvc;
using Tenders.Application.CommandQuery;

namespace Tenders.Web.Controllers;

public class BaseController(RequestHandler requestHandler) : ControllerBase
{
    protected async Task<IActionResult> HandleOkResponse<T>(RequestBase<T> request)
    {
        await requestHandler.ProcessRequest(request);
        return new OkResult();
    }

    protected async Task<IActionResult> HandleOkObjectResponse<T>(RequestBase<T> request, Func<T, object>? formatter = null)
    {
        var result = await requestHandler.ProcessRequest(request);
        var formattedResult = formatter == null ? result : formatter(result);

        return new OkObjectResult(formattedResult);
    }
}