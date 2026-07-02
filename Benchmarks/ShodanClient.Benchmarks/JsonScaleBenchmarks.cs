using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using ShodanClient.Infrastructure.Search.Wire;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Benchmarks;

/// <summary>
///     Extends the single-object comparison in <see cref="JsonDeserializationBenchmarks" /> to a
///     realistic full search-results page: compares source-generated (AOT/trim-safe) deserialization
///     via <see cref="ShodanJsonContext" /> against the reflection-based <see cref="JsonSerializer" />
///     path for a <see cref="SearchResponse" /> payload approximating one page of
///     <c>GET /shodan/host/search</c> results (~20 matches plus a populated facets dictionary).
/// </summary>
[MemoryDiagnoser]
public class JsonScaleBenchmarks
{
	private const int MatchCount = 20;
	private const int FacetItemCount = 10;

	private static readonly string[] FacetNames = ["port", "org", "country"];

	private static readonly string PageJson = BuildPageJson();

	private static readonly JsonSerializerOptions ReflectionOptions = new(JsonSerializerDefaults.Web);

	[Benchmark(Baseline = true, Description = "Source-gen (ShodanJsonContext)")]
	public object? SourceGenerated() =>
		JsonSerializer.Deserialize(PageJson, ShodanJsonContext.Default.SearchResponse);

	[Benchmark(Description = "Reflection (JsonSerializer.Deserialize<T>)")]
	[UnconditionalSuppressMessage("AOT", "IL2026",
		Justification = "Deliberately benchmarking the reflection path against the source-gen path above.")]
	[UnconditionalSuppressMessage("AOT", "IL3050",
		Justification = "Deliberately benchmarking the reflection path against the source-gen path above.")]
	public object? Reflection() =>
		JsonSerializer.Deserialize<SearchResponse>(PageJson, ReflectionOptions);

	private static string BuildPageJson()
	{
		var matches = new StringBuilder();
		for (var i = 0; i < MatchCount; i++)
		{
			if (i > 0)
			{
				matches.Append(',');
			}

			matches.Append(BuildMatchJson(i));
		}

		var facets = new StringBuilder();
		foreach (var facetName in FacetNames)
		{
			if (facets.Length > 0)
			{
				facets.Append(',');
			}

			var facetEntry = "\"" + facetName + "\": [" + BuildFacetItemsJson(facetName) + "]";
			facets.Append(facetEntry);
		}

		return $$"""
		         {
		           "matches": [{{matches}}],
		           "total": {{MatchCount}},
		           "facets": {
		             {{facets}}
		           }
		         }
		         """;
	}

	private static string BuildMatchJson(int index)
	{
		var ip = $"8.8.{index / 256}.{index % 256}";
		var port = 8000 + index;
		return $$"""
		         {
		           "ip_str": "{{ip}}",
		           "port": {{port}},
		           "transport": "tcp",
		           "hostnames": ["host{{index}}.example.com"],
		           "domains": ["example.com"],
		           "org": "Example Org {{index}}",
		           "isp": "Example ISP",
		           "asn": "AS{{15000 + index}}",
		           "timestamp": "2024-09-29T09:39:45.813661",
		           "location": { "country_code": "US", "country_name": "United States", "city": "Mountain View" },
		           "_shodan": { "id": "abc-{{index}}", "module": "https", "crawler": "crawler-1", "ptr": false },
		           "http": {
		             "status": 200,
		             "title": "Example {{index}}",
		             "server": "nginx",
		             "headers": { "Content-Type": "text/html", "Server": "nginx" }
		           },
		           "ssl": {
		             "versions": ["TLSv1.2", "TLSv1.3"],
		             "cipher": { "version": "TLSv1.3", "bits": 256, "name": "TLS_AES_256_GCM_SHA384" },
		             "cert": { "issued": "20240101000000Z", "expires": "20250101000000Z", "expired": false }
		           }
		         }
		         """;
	}

	private static string BuildFacetItemsJson(string facetName)
	{
		var items = new StringBuilder();
		for (var i = 0; i < FacetItemCount; i++)
		{
			if (i > 0)
			{
				items.Append(',');
			}

			var count = (FacetItemCount - i).ToString(CultureInfo.InvariantCulture);
			var item = "{ \"count\": " + count + ", \"value\": \"" + facetName + "-" +
					   i.ToString(CultureInfo.InvariantCulture) + "\" }";
			items.Append(item);
		}

		return items.ToString();
	}
}
