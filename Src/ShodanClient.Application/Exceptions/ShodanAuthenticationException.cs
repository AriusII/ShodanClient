using System.Net;

namespace ShodanClient.Application.Exceptions;

/// <summary>
///     Thrown when Shodan rejects the request because the API key is missing or invalid
///     (HTTP 401). Verify <c>ShodanClientOptions.ApiKey</c>.
/// </summary>
public sealed class ShodanAuthenticationException : ShodanApiException
{
	/// <summary>Creates an authentication exception (HTTP 401).</summary>
	public ShodanAuthenticationException(
		string? apiMessage = null,
		Uri? requestUri = null,
		string? surface = null,
		Exception? innerException = null)
		: base(HttpStatusCode.Unauthorized, apiMessage, requestUri, surface, innerException)
	{
	}
}
