using System.Globalization;
using System.Net;

namespace ShodanClient.Application.Exceptions;

/// <summary>
///     Thrown when the Shodan API responds with a non-success HTTP status code. More specific
///     subtypes (<see cref="ShodanAuthenticationException" />, <see cref="ShodanNotFoundException" />,
///     <see cref="ShodanRateLimitException" />, …) are raised where the status code is recognized.
/// </summary>
public class ShodanApiException : ShodanException
{
	/// <summary>Creates an API exception.</summary>
	/// <param name="statusCode">The HTTP status code returned by Shodan.</param>
	/// <param name="apiMessage">The <c>error</c>/<c>detail</c> message from the response body, if any.</param>
	/// <param name="requestUri">The request URI (with the API key redacted).</param>
	/// <param name="surface">The logical Shodan surface name (e.g. <c>Rest</c>, <c>Streaming</c>).</param>
	/// <param name="innerException">The underlying exception, if any.</param>
	public ShodanApiException(
		HttpStatusCode statusCode,
		string? apiMessage = null,
		Uri? requestUri = null,
		string? surface = null,
		Exception? innerException = null)
		: base(BuildMessage(statusCode, apiMessage, surface), innerException!)
	{
		StatusCode = statusCode;
		ApiMessage = apiMessage;
		RequestUri = requestUri;
		Surface = surface;
	}

	/// <summary>The HTTP status code returned by Shodan.</summary>
	public HttpStatusCode StatusCode { get; }

	/// <summary>The error message extracted from the response body, if present.</summary>
	public string? ApiMessage { get; }

	/// <summary>The request URI, with the API key redacted.</summary>
	public Uri? RequestUri { get; }

	/// <summary>The logical Shodan surface the request targeted.</summary>
	public string? Surface { get; }

	private static string BuildMessage(HttpStatusCode statusCode, string? apiMessage, string? surface)
	{
		var code = (int)statusCode;
		var where = string.IsNullOrEmpty(surface) ? "Shodan" : $"Shodan {surface}";
		return string.IsNullOrEmpty(apiMessage)
			? string.Format(CultureInfo.InvariantCulture, "{0} API request failed with status {1} ({2}).", where, code,
				statusCode)
			: string.Format(CultureInfo.InvariantCulture, "{0} API request failed with status {1} ({2}): {3}", where,
				code, statusCode, apiMessage);
	}
}
