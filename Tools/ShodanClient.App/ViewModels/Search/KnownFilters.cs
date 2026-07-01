using System.Collections.Frozen;

namespace ShodanClient.App.ViewModels.Search;

/// <summary>
///     Curated tooltip/hint metadata for one Shodan search filter.
/// </summary>
/// <param name="Name">The filter name, e.g. <c>port</c>.</param>
/// <param name="Description">A short human-readable description of what the filter does.</param>
/// <param name="Example">An example usage, e.g. <c>port:443</c>.</param>
public sealed record KnownFilterInfo(string Name, string Description, string Example);

/// <summary>
///     Static lookup of <see cref="KnownFilterInfo" /> for the highest-value Shodan search filters,
///     used to enrich the query builder's tooltips/hints. This is progressive enhancement only:
///     <see cref="ShodanClient.Application.Abstractions.Services.ISearchService.GetFiltersAsync" />
///     remains the full, authoritative source of every filter name the API actually accepts.
/// </summary>
public static class KnownFilters
{
	/// <summary>
	///     The curated filters, in their authored priority order (highest-value first). This is the
	///     single source of truth: <see cref="All" /> is derived from it, and consumers that need a
	///     stable "top N" (e.g. Trends' quick-insert hints) should read from this list rather than
	///     <see cref="All" />.Values, whose enumeration order is unspecified.
	/// </summary>
	public static IReadOnlyList<KnownFilterInfo> Ordered { get; } =
	[
		new("port", "The port number the service was found running on.", "port:443"),
		new("country", "Two-letter ISO 3166-1 alpha-2 country code.", "country:US"),
		new("city", "City name; quote multi-word names.", "city:\"San Francisco\""),
		new("org", "The organization that owns the netblock.", "org:\"Google LLC\""),
		new("asn", "Autonomous System Number, prefixed with AS.", "asn:AS15169"),
		new("product", "Identified software/product name.", "product:nginx"),
		new("version", "Identified product version string.", "version:1.18.0"),
		new("os", "Fingerprinted operating system.", "os:\"Windows Server 2019\""),
		new("hostname", "Full or partial hostname; supports a leading wildcard.", "hostname:example.com"),
		new("net", "IP range in CIDR notation.", "net:203.0.113.0/24"),
		new("before", "Only results first seen before this date (dd/mm/yyyy).", "before:01/01/2024"),
		new("after", "Only results first seen after this date (dd/mm/yyyy).", "after:01/01/2024"),
		new("has_vuln", "Only results with at least one known CVE.", "has_vuln:true"),
		new("vuln", "Results affected by a specific CVE identifier.", "vuln:CVE-2021-44228"),
		new("ssl.cert.subject.cn", "SSL certificate subject common name.", "ssl.cert.subject.cn:*.example.com"),
		new("http.title", "The HTML page's <title> content.", "http.title:\"Login\""),
		new("http.status", "HTTP response status code.", "http.status:200")
	];

	/// <summary>The curated filters, keyed by name (case-insensitive), for O(1) lookup by <see cref="TryGet" />.</summary>
	public static FrozenDictionary<string, KnownFilterInfo> All { get; } =
		Ordered.ToFrozenDictionary(info => info.Name, StringComparer.OrdinalIgnoreCase);

	/// <summary>Looks up curated metadata for a filter name, if any is available.</summary>
	public static KnownFilterInfo? TryGet(string filterName) => All.GetValueOrDefault(filterName);
}
