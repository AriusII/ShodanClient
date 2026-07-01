using BenchmarkDotNet.Attributes;
using ShodanClient.Infrastructure.Http.Routing;

namespace ShodanClient.Benchmarks;

/// <summary>
///     Measures full route-construction cost (query-string building plus interpolated-path
///     allocation) through <see cref="ShodanRoutes" /> for three representative route shapes:
///     a search route with query + facets + page, a simple single-id route with no query string
///     at all, and a route with several optional query parameters all populated at once.
/// </summary>
[MemoryDiagnoser]
public class RouteBuildingBenchmarks
{
	private const string SearchQuery = "product:nginx port:443 country:US";
	private const string Facets = "country,org,port";
	private const int SearchPage = 3;

	private const string AlertId = "abc-123-def-456";

	private const string Domain = "example.com";
	private const string DnsType = "A";
	private const int DnsPage = 2;

	[Benchmark(Description = "Search.SearchHosts (query + facets + page)")]
	public string SearchHosts() =>
		ShodanRoutes.Search.SearchHosts(SearchQuery, Facets, SearchPage).RelativePath;

	[Benchmark(Description = "Alerts.Get (single id, no query string)")]
	public string AlertGet() =>
		ShodanRoutes.Alerts.Get(AlertId).RelativePath;

	[Benchmark(Description = "Dns.Domain (history + type + page all set)")]
	public string DnsDomain() =>
		ShodanRoutes.Dns.Domain(Domain, true, DnsType, DnsPage).RelativePath;
}
