using System.Net;

namespace ShodanClient.Application.Exceptions;

/// <summary>
///     Thrown when the endpoint or feature is not available on the account's plan, or the
///     request is otherwise forbidden (HTTP 403).
/// </summary>
public sealed class ShodanAccessDeniedException : ShodanApiException
{
	/// <summary>Creates an access-denied exception (HTTP 403).</summary>
	public ShodanAccessDeniedException(
		string? apiMessage = null,
		Uri? requestUri = null,
		string? surface = null,
		Exception? innerException = null)
		: base(HttpStatusCode.Forbidden, apiMessage, requestUri, surface, innerException)
	{
	}
}
