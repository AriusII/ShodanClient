using ShodanClient.Domain.Search;

namespace ShodanClient.Application.Abstractions.Services;

/// <summary>
///     Consuming the long-lived, newline-delimited-JSON banner feeds on the Streaming API. Exposed on
///     the client as <c>IShodanClient.Stream</c>.
/// </summary>
public interface IStreamService
{
	/// <summary>Streams the firehose of every banner Shodan collects.</summary>
	/// <param name="cancellationToken">A token to stop the enumeration.</param>
	IAsyncEnumerable<Banner> StreamAllBannersAsync(CancellationToken cancellationToken = default);

	/// <summary>Streams banners collected for the given Autonomous System Numbers.</summary>
	/// <param name="asns">The ASNs to filter by, e.g. <c>"3303"</c>, <c>"32475"</c>.</param>
	/// <param name="cancellationToken">A token to stop the enumeration.</param>
	IAsyncEnumerable<Banner> StreamByAsnAsync(IEnumerable<string> asns, CancellationToken cancellationToken = default);

	/// <summary>Streams banners collected for the given countries.</summary>
	/// <param name="countryCodes">The 2-letter ISO country codes to filter by.</param>
	/// <param name="cancellationToken">A token to stop the enumeration.</param>
	IAsyncEnumerable<Banner> StreamByCountriesAsync(
		IEnumerable<string> countryCodes,
		CancellationToken cancellationToken = default);

	/// <summary>Streams banners collected on the given ports.</summary>
	/// <param name="ports">The ports to filter by.</param>
	/// <param name="cancellationToken">A token to stop the enumeration.</param>
	IAsyncEnumerable<Banner> StreamByPortsAsync(IEnumerable<int> ports, CancellationToken cancellationToken = default);

	/// <summary>Streams banners affected by the given vulnerabilities.</summary>
	/// <param name="cveIds">The CVE identifiers to filter by.</param>
	/// <param name="cancellationToken">A token to stop the enumeration.</param>
	IAsyncEnumerable<Banner> StreamByVulnerabilitiesAsync(
		IEnumerable<string> cveIds,
		CancellationToken cancellationToken = default);

	/// <summary>Streams banners matching a case-sensitive Shodan search query.</summary>
	/// <param name="query">The Shodan query to filter by.</param>
	/// <param name="cancellationToken">A token to stop the enumeration.</param>
	IAsyncEnumerable<Banner> StreamByQueryAsync(string query, CancellationToken cancellationToken = default);

	/// <summary>Streams banners across every one of the caller's network alerts.</summary>
	/// <param name="cancellationToken">A token to stop the enumeration.</param>
	IAsyncEnumerable<Banner> StreamAlertsAsync(CancellationToken cancellationToken = default);

	/// <summary>Streams banners for a single network alert.</summary>
	/// <param name="alertId">The identifier of the network alert.</param>
	/// <param name="cancellationToken">A token to stop the enumeration.</param>
	IAsyncEnumerable<Banner> StreamAlertAsync(string alertId, CancellationToken cancellationToken = default);
}
