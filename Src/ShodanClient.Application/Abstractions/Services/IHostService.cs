using ShodanClient.Domain.Search;

namespace ShodanClient.Application.Abstractions.Services;

/// <summary>
///     Host lookups. Exposed on the client as <c>IShodanClient.Hosts</c>.
/// </summary>
public interface IHostService
{
	/// <summary>
	///     Returns every service Shodan has found on the given host.
	/// </summary>
	/// <param name="ip">The host IP address (IPv4 or IPv6).</param>
	/// <param name="history">Include all historical banners, not just the most recent.</param>
	/// <param name="minify">Return only ports and general host information, omitting banners.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>The aggregated host information.</returns>
	Task<Host> GetAsync(
		string ip,
		bool history = false,
		bool minify = false,
		CancellationToken cancellationToken = default);
}
