using BenchmarkDotNet.Attributes;
using ShodanClient.Infrastructure.Http.Routing;

namespace ShodanClient.Benchmarks;

/// <summary>
///     Compares the allocation-conscious <see cref="QueryStringBuilder" /> (backed by a pooled
///     interpolated-string handler) against naive string concatenation for a typical search route.
/// </summary>
[MemoryDiagnoser]
public class QueryStringBuilderBenchmarks
{
	private const string Query = "product:nginx port:443 country:US";
	private const string Facets = "country,org,port";
	private const int Page = 3;

	[Benchmark(Baseline = true, Description = "QueryStringBuilder")]
	public string WithQueryStringBuilder()
	{
		var builder = new QueryStringBuilder(Query.Length + 32);
		builder.Add("query", Query);
		builder.AddIfPresent("facets", Facets);
		if (Page > 1)
		{
			builder.Add("page", Page);
		}

		return builder.Build();
	}

	[Benchmark(Description = "String concatenation")]
	public string WithStringConcatenation()
	{
		var result = "?query=" + Uri.EscapeDataString(Query);
		if (!string.IsNullOrEmpty(Facets))
		{
			result += "&facets=" + Uri.EscapeDataString(Facets);
		}

		if (Page > 1)
		{
			result += "&page=" + Page;
		}

		return result;
	}
}
