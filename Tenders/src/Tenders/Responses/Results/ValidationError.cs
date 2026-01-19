using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Tenders.Web.Responses.ErrorHandling;

namespace Tenders.Web.Responses.Results;

public class ValidationError : BadRequestObjectResult
{
    public ValidationError(string? content, ValidationException validationException) : base(content)
    {
        var failedValidations = validationException.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                x => x.Key,
                x => x.Select(x => $"{x.ErrorCode}: {x.ErrorMessage}").ToList());

        Value = new ValidationErrorResponseModel(failedValidations);
    }
}