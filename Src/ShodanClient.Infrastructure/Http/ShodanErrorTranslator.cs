using System.Net;
using ShodanClient.Application.Exceptions;

namespace ShodanClient.Infrastructure.Http;

/// <summary>
///     Maps an HTTP status code (and any parsed error message) to the most specific
///     <see cref="ShodanApiException" /> subtype, so callers can catch a meaningful exception type.
/// </summary>
internal static class ShodanErrorTranslator
{
	public static ShodanApiException Translate(
		HttpStatusCode statusCode,
		string? apiMessage,
		Uri? requestUri,
		string surface,
		TimeSpan? retryAfter)
	{
		return statusCode switch
		{
			HttpStatusCode.Unauthorized =>
				new ShodanAuthenticationException(apiMessage, requestUri, surface),
			HttpStatusCode.Forbidden =>
				new ShodanAccessDeniedException(apiMessage, requestUri, surface),
			HttpStatusCode.NotFound =>
				new ShodanNotFoundException(apiMessage, requestUri, surface),
			HttpStatusCode.TooManyRequests =>
				new ShodanRateLimitException(retryAfter, apiMessage, requestUri, surface),
			>= HttpStatusCode.InternalServerError =>
				new ShodanServerException(statusCode, apiMessage, requestUri, surface),
			_ => new ShodanApiException(statusCode, apiMessage, requestUri, surface)
		};
	}
}
