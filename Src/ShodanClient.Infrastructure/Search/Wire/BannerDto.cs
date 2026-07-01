using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.Search.Wire;

/// <summary>Wire shape of a single service banner (host <c>data[]</c> / search <c>matches[]</c>).</summary>
internal sealed class BannerDto
{
	[JsonPropertyName("ip_str")] public string? IpStr { get; set; }

	public long? Ip { get; set; }

	[JsonPropertyName("ipv6")] public string? IpV6 { get; set; }

	public int Port { get; set; }

	public string? Transport { get; set; }

	public string? Protocol { get; set; }

	public string? Data { get; set; }

	public string? Product { get; set; }

	public string? Version { get; set; }

	public string? Info { get; set; }

	[JsonPropertyName("os")] public string? Os { get; set; }

	public string[]? Cpe { get; set; }

	[JsonPropertyName("cpe23")] public string[]? Cpe23 { get; set; }

	public string[]? Hostnames { get; set; }

	public string[]? Domains { get; set; }

	public string[]? Tags { get; set; }

	public string? Asn { get; set; }

	public string? Isp { get; set; }

	public string? Org { get; set; }

	public string? Timestamp { get; set; }

	public long? Hash { get; set; }

	public LocationDto? Location { get; set; }

	[JsonPropertyName("_shodan")] public ShodanMetaDto? Shodan { get; set; }

	public Dictionary<string, VulnDto>? Vulns { get; set; }

	public HttpDto? Http { get; set; }

	public SslDto? Ssl { get; set; }

	[JsonPropertyName("dns")] public DnsModuleDto? Dns { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}

/// <summary>Wire shape of the <c>location</c> block.</summary>
internal sealed class LocationDto
{
	public string? City { get; set; }

	[JsonPropertyName("region_code")] public string? RegionCode { get; set; }

	[JsonPropertyName("area_code")] public int? AreaCode { get; set; }

	[JsonPropertyName("postal_code")] public string? PostalCode { get; set; }

	[JsonPropertyName("dma_code")] public int? DmaCode { get; set; }

	[JsonPropertyName("country_code")] public string? CountryCode { get; set; }

	[JsonPropertyName("country_code3")] public string? CountryCode3 { get; set; }

	[JsonPropertyName("country_name")] public string? CountryName { get; set; }

	public double? Latitude { get; set; }

	public double? Longitude { get; set; }
}

/// <summary>Wire shape of the <c>_shodan</c> collection-metadata block.</summary>
internal sealed class ShodanMetaDto
{
	public string? Id { get; set; }

	public string? Module { get; set; }

	public string? Crawler { get; set; }

	public bool Ptr { get; set; }

	public Dictionary<string, JsonElement>? Options { get; set; }

	public ShodanMetaAlertDto? Alert { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}

/// <summary>
///     Wire shape of the <c>_shodan.alert</c> block, present on <c>/shodan/alert</c> and
///     <c>/shodan/alert/{id}</c> streaming endpoints to identify which alert produced the banner.
/// </summary>
internal sealed class ShodanMetaAlertDto
{
	public string? Id { get; set; }

	public string? Name { get; set; }
}

/// <summary>Wire shape of the <c>dns</c> collection-module block.</summary>
internal sealed class DnsModuleDto
{
	[JsonPropertyName("resolver_hostname")]
	public string? ResolverHostname { get; set; }

	public bool? Recursive { get; set; }

	[JsonPropertyName("resolver_id")] public string? ResolverId { get; set; }

	public string? Software { get; set; }
}

/// <summary>Wire shape of a <c>vulns</c> map value.</summary>
internal sealed class VulnDto
{
	public bool Verified { get; set; }

	public double? Cvss { get; set; }

	[JsonPropertyName("cvss_version")] public int? CvssVersion { get; set; }

	public double? Epss { get; set; }

	[JsonPropertyName("ranking_epss")] public double? RankingEpss { get; set; }

	public bool Kev { get; set; }

	public string? Summary { get; set; }

	public string[]? References { get; set; }
}
