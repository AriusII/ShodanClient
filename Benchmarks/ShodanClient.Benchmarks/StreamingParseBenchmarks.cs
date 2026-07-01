using System.Text;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Benchmarks;

/// <summary>
///     Measures the throughput of parsing newline-delimited JSON (NDJSON) banner objects the same
///     way <c>ShodanChannel.StreamNdjsonAsync</c> does: <c>JsonSerializer.DeserializeAsyncEnumerable</c>
///     against the source-generated <see cref="ShodanJsonContext" /> with <c>topLevelValues: true</c>, over a
///     stream of ~200 concatenated banner objects (one per line, no wrapping array). This approximates the
///     marginal per-item cost of the live streaming path without needing a real HTTP connection.
/// </summary>
[MemoryDiagnoser]
public class StreamingParseBenchmarks
{
	private const int BannerCount = 200;

	private const string BannerJsonLine = """
	                                      {"ip_str":"8.8.8.8","port":443,"transport":"tcp","hostnames":["dns.google"],"domains":["google"],"org":"Google LLC","isp":"Google LLC","asn":"AS15169","timestamp":"2024-09-29T09:39:45.813661","location":{"country_code":"US","country_name":"United States","city":"Mountain View"},"_shodan":{"id":"abc-123","module":"https","crawler":"crawler-1","ptr":false},"http":{"status":200,"title":"Example","server":"nginx","headers":{"Content-Type":"text/html","Server":"nginx"}},"ssl":{"versions":["TLSv1.2","TLSv1.3"],"cipher":{"version":"TLSv1.3","bits":256,"name":"TLS_AES_256_GCM_SHA384"},"cert":{"issued":"20240101000000Z","expires":"20250101000000Z","expired":false}}}
	                                      """;

	// Populated once in GlobalSetup; each invocation wraps a fresh, non-shared MemoryStream around this
	// buffer so the stream is always read from the beginning, regardless of how BenchmarkDotNet batches
	// invocations within a single measured iteration (IterationSetup only runs once per iteration, not
	// once per invocation, so it would silently under-measure once the pilot stage picks an unroll factor
	// greater than one).
	private byte[] _ndjsonBytes = [];

	[GlobalSetup]
	public void GlobalSetup()
	{
		var builder = new StringBuilder();
		for (var i = 0; i < BannerCount; i++)
		{
			builder.Append(BannerJsonLine).Append('\n');
		}

		_ndjsonBytes = Encoding.UTF8.GetBytes(builder.ToString());
	}

	[Benchmark(Description = "DeserializeAsyncEnumerable (NDJSON, topLevelValues: true)")]
	public async Task<int> StreamNdjsonAsync()
	{
		using var stream = new MemoryStream(_ndjsonBytes, false);

		var count = 0;
		var sequence = JsonSerializer.DeserializeAsyncEnumerable(stream, ShodanJsonContext.Default.BannerDto, true);
		await foreach (var item in sequence)
		{
			if (item is not null)
			{
				count++;
			}
		}

		return count;
	}
}
