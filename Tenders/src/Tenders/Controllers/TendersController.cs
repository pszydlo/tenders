using System.Net;
using Microsoft.AspNetCore.Mvc;
using Tenders.Application.CommandQuery;
using Tenders.Application.CommandQuery.Queries;
using Tenders.Application.Dtos;
using Tenders.Domain.Models;
using Tenders.Web.Responses.ErrorHandling;
using Tenders.Web.Responses.Results;
using static Tenders.Web.WebConstants;

namespace Tenders.Web.Controllers;

[ApiController]
[Route(TendersRoute)]
public class TendersController(
    RequestHandler requestHandler)
    : BaseController(requestHandler)
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<TenderDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ServiceUnavailableResponseModel), (int)HttpStatusCode.ServiceUnavailable)]
    public Task<IActionResult> Get(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 100,
        [FromQuery] decimal? minPriceEur = null,
        [FromQuery] decimal? maxPriceEur = null,
        [FromQuery] DateOnly? dateFrom = null,
        [FromQuery] DateOnly? dateTo = null,
        [FromQuery] int? supplierId = null,
        [FromQuery] TenderOrderBy orderBy = TenderOrderBy.Date,
        [FromQuery] OrderDirection orderDirection = OrderDirection.Desc)
        => HandleOkObjectResponse(new GetTenders.Query(
            minPriceEur,
            maxPriceEur,
            dateFrom,
            dateTo,
            supplierId,
            pageSize,
            pageNumber,
            orderBy,
            orderDirection));

    /// <summary>
    /// Retrieves a single tender by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TenderDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(ServiceUnavailableResponseModel), (int)HttpStatusCode.ServiceUnavailable)]

    public Task<IActionResult> GetById([FromRoute] int id)
    => HandleOkObjectResponse(new GetTenderById.Query(id));
}
