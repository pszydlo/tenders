namespace Tenders.Web.Responses.ErrorHandling;

public record ValidationErrorResponseModel(Dictionary<string, List<string>> ValidationErrors);
