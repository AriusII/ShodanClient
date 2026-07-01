using System.Net;

namespace ShodanClient.Application.Exceptions;

/// <summary>
///     Thrown when the Shodan rate limit has been exceeded (HTTP 429). Shodan's REST API is
///     throttled to roughly one request per second by default; the client already paces requests,
///     but a burst from multiple processes sharing a key can still trip this.
/// </summary>
public sealed class ShodanRateLimitException : ShodanApiException
{
	/// <summary>Creates a rate-limit exception (HTTP 429).</summary>
	/// <param name="retryAfter">The server-suggested delay before retrying, if provided.</param>
	public ShodanRateLimitException(
		TimeSpan? retryAfter = null,
		string? apiMessage = null,
		Uri? requestUri = null,
		string? surface = null,
		Exception? innerException = null)
		: base(HttpStatusCode.TooManyRequests, apiMessage, requestUri, surface, innerException)
	{
		RetryAfter = retryAfter;
	}

	/// <summary>The server-suggested delay before retrying, if the response included one.</summary>
	public TimeSpan? RetryAfter { get; }
}
