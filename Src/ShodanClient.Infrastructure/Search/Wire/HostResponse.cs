using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.Search.Wire;

/// <summary>Wire shape of the <c>GET /shodan/host/{ip}</c> response.</summary>
internal sealed class HostResponse
{
	[JsonPropertyName("ip_str")] public string? IpStr { get; set; }

	public long? Ip { get; set; }

	public int[]? Ports { get; set; }

	public string[]? Hostnames { get; set; }

	public string[]? Domains { get; set; }

	public string[]? Tags { get; set; }

	// At the host level, vulns is a flat array of CVE ids (unlike the map on a banner).
	public string[]? Vulns { get; set; }

	public string? Asn { get; set; }

	public string? Isp { get; set; }

	public string? Org { get; set; }

	[JsonPropertyName("os")] public string? Os { get; set; }

	[JsonPropertyName("last_update")] public string? LastUpdate { get; set; }

	public string? City { get; set; }

	[JsonPropertyName("region_code")] public string? RegionCode { get; set; }

	[JsonPropertyName("postal_code")] public string? PostalCode { get; set; }

	[JsonPropertyName("area_code")] public int? AreaCode { get; set; }

	[JsonPropertyName("dma_code")] public int? DmaCode { get; set; }

	[JsonPropertyName("country_code")] public string? CountryCode { get; set; }

	[JsonPropertyName("country_code3")] public string? CountryCode3 { get; set; }

	[JsonPropertyName("country_name")] public string? CountryName { get; set; }

	public double? Latitude { get; set; }

	public double? Longitude { get; set; }

	public BannerDto[]? Data { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}
