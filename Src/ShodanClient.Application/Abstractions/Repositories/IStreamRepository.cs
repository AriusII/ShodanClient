using ShodanClient.Domain.Search;

namespace ShodanClient.Application.Abstractions.Repositories;

/// <summary>Transport-level access to the newline-delimited-JSON banner feeds on the Streaming API.</summary>
internal interface IStreamRepository
{
	/// <summary>Streams the firehose of every collected banner (<c>GET /shodan/banners</c>).</summary>
	IAsyncEnumerable<Banner> StreamAllBannersAsync(CancellationToken cancellationToken);

	/// <summary>Streams banners for the given ASNs (<c>GET /shodan/asn/{asn}</c>).</summary>
	IAsyncEnumerable<Banner> StreamByAsnAsync(IEnumerable<string> asns, CancellationToken cancellationToken);

	/// <summary>Streams banners for the given 2-letter country codes (<c>GET /shodan/countries/{countries}</c>).</summary>
	IAsyncEnumerable<Banner> StreamByCountriesAsync(IEnumerable<string> countryCodes,
		CancellationToken cancellationToken);

	/// <summary>Streams banners for the given ports (<c>GET /shodan/ports/{ports}</c>).</summary>
	IAsyncEnumerable<Banner> StreamByPortsAsync(IEnumerable<int> ports, CancellationToken cancellationToken);

	/// <summary>Streams banners affected by the given CVE identifiers (<c>GET /shodan/vulns/{vulns}</c>).</summary>
	IAsyncEnumerable<Banner> StreamByVulnerabilitiesAsync(IEnumerable<string> cveIds,
		CancellationToken cancellationToken);

	/// <summary>Streams banners matching a case-sensitive Shodan query (<c>GET /shodan/custom</c>).</summary>
	IAsyncEnumerable<Banner> StreamByQueryAsync(string query, CancellationToken cancellationToken);

	/// <summary>Streams banners across every one of the caller's network alerts (<c>GET /shodan/alert</c>).</summary>
	IAsyncEnumerable<Banner> StreamAlertsAsync(CancellationToken cancellationToken);

	/// <summary>Streams banners for a single network alert (<c>GET /shodan/alert/{id}</c>).</summary>
	IAsyncEnumerable<Banner> StreamAlertAsync(string alertId, CancellationToken cancellationToken);
}
