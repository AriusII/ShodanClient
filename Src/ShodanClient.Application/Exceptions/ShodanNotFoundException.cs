using System.Net;

namespace ShodanClient.Application.Exceptions;

/// <summary>
///     Thrown when Shodan has no information for the requested resource (HTTP 404) — for
///     example a host with no collected data, an unknown scan id, or an unknown IP on InternetDB.
/// </summary>
public sealed class ShodanNotFoundException : ShodanApiException
{
	/// <summary>Creates a not-found exception (HTTP 404).</summary>
	public ShodanNotFoundException(
		string? apiMessage = null,
		Uri? requestUri = null,
		string? surface = null,
		Exception? innerException = null)
		: base(HttpStatusCode.NotFound, apiMessage, requestUri, surface, innerException)
	{
	}
}
