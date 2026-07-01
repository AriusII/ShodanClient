using System.Net;

namespace ShodanClient.Application.Exceptions;

/// <summary>
///     Thrown when Shodan returns a server-side error (HTTP 5xx). These are typically transient;
///     the resilience pipeline retries them before the exception surfaces.
/// </summary>
public sealed class ShodanServerException : ShodanApiException
{
	/// <summary>Creates a server-error exception (HTTP 5xx).</summary>
	public ShodanServerException(
		HttpStatusCode statusCode = HttpStatusCode.InternalServerError,
		string? apiMessage = null,
		Uri? requestUri = null,
		string? surface = null,
		Exception? innerException = null)
		: base(statusCode, apiMessage, requestUri, surface, innerException)
	{
	}
}
