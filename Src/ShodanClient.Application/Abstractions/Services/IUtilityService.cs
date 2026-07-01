namespace ShodanClient.Application.Abstractions.Services;

/// <summary>
///     Small diagnostic utilities exposed by Shodan. Exposed on the client as
///     <c>IShodanClient.Tools</c>.
/// </summary>
public interface IUtilityService
{
	/// <summary>
	///     Returns the HTTP headers that the caller sends, as seen by Shodan
	///     (<c>GET /tools/httpheaders</c>).
	/// </summary>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<IReadOnlyDictionary<string, string>> GetHttpHeadersAsync(CancellationToken cancellationToken = default);

	/// <summary>Returns the caller's public IP address (<c>GET /tools/myip</c>).</summary>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<string> GetMyIpAsync(CancellationToken cancellationToken = default);
}
