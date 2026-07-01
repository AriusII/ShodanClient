using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using ShodanClient.Infrastructure.Search.Wire;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Benchmarks;

/// <summary>
///     Compares source-generated (AOT/trim-safe) deserialization via <see cref="ShodanJsonContext" />
///     against the reflection-based <see cref="JsonSerializer" /> path for a realistic banner payload.
/// </summary>
[MemoryDiagnoser]
public class JsonDeserializationBenchmarks
{
	private const string BannerJson = """
	                                  {
	                                    "ip_str": "8.8.8.8",
	                                    "port": 443,
	                                    "transport": "tcp",
	                                    "hostnames": ["dns.google"],
	                                    "domains": ["google"],
	                                    "org": "Google LLC",
	                                    "isp": "Google LLC",
	                                    "asn": "AS15169",
	                                    "timestamp": "2024-09-29T09:39:45.813661",
	                                    "location": { "country_code": "US", "country_name": "United States", "city": "Mountain View" },
	                                    "_shodan": { "id": "abc-123", "module": "https", "crawler": "crawler-1", "ptr": false },
	                                    "http": {
	                                      "status": 200,
	                                      "title": "Example",
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

	private static readonly JsonSerializerOptions ReflectionOptions = new(JsonSerializerDefaults.Web);

	[Benchmark(Baseline = true, Description = "Source-gen (ShodanJsonContext)")]
	public object? SourceGenerated() =>
		JsonSerializer.Deserialize(BannerJson, ShodanJsonContext.Default.BannerDto);

	[Benchmark(Description = "Reflection (JsonSerializer.Deserialize<T>)")]
	[UnconditionalSuppressMessage("AOT", "IL2026",
		Justification = "Deliberately benchmarking the reflection path against the source-gen path above.")]
	[UnconditionalSuppressMessage("AOT", "IL3050",
		Justification = "Deliberately benchmarking the reflection path against the source-gen path above.")]
	public object? Reflection() =>
		JsonSerializer.Deserialize<BannerDto>(BannerJson, ReflectionOptions);
}
