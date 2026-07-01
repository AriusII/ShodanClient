using ShodanClient.Domain.Common;

namespace ShodanClient.Domain.Search;

/// <summary>
///     Aggregated view of a single host returned by <c>GET /shodan/host/{ip}</c>: its open ports,
///     hostnames, geolocation and every collected service banner.
/// </summary>
public sealed record Host
{
	/// <summary>Textual IP address (<c>ip_str</c>).</summary>
	public required string IpString { get; init; }

	/// <summary>Integer form of the IPv4 address, if applicable (<c>ip</c>).</summary>
	public long? Ip { get; init; }

	/// <summary>Open ports discovered on the host (<c>ports</c>).</summary>
	public IReadOnlyList<int> Ports { get; init; } = [];

	/// <summary>Hostnames pointing at the host (<c>hostnames</c>).</summary>
	public IReadOnlyList<string> Hostnames { get; init; } = [];

	/// <summary>Registered domains for the host (<c>domains</c>).</summary>
	public IReadOnlyList<string> Domains { get; init; } = [];

	/// <summary>Classification tags (<c>tags</c>).</summary>
	public IReadOnlyList<string> Tags { get; init; } = [];

	/// <summary>CVE identifiers affecting the host (<c>vulns</c> — a flat array at the host level).</summary>
	public IReadOnlyList<string> Vulnerabilities { get; init; } = [];

	/// <summary>Autonomous System Number (<c>asn</c>).</summary>
	public string? Asn { get; init; }

	/// <summary>Internet Service Provider (<c>isp</c>).</summary>
	public string? Isp { get; init; }

	/// <summary>Owning organization (<c>org</c>).</summary>
	public string? Organization { get; init; }

	/// <summary>Operating system, if fingerprinted (<c>os</c>).</summary>
	public string? OperatingSystem { get; init; }

	/// <summary>When the host was last updated (<c>last_update</c>).</summary>
	public DateTimeOffset? LastUpdate { get; init; }

	/// <summary>Geographic location aggregated for the host.</summary>
	public GeoLocation? Location { get; init; }

	/// <summary>Every collected service banner for the host (<c>data</c>).</summary>
	public IReadOnlyList<Banner> Services { get; init; } = [];
}
