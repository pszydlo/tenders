namespace Tenders.Application.Exceptions;

public sealed class ServiceUnavailableException(
    string message = "Service is temporarily unavailable.",
    int? retryAfterSeconds = null) : Exception(message)
{
    public int? RetryAfterSeconds { get; } = retryAfterSeconds;
}
