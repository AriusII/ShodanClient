using ShodanClient.Domain.Common;

namespace ShodanClient.Domain.Search;

/// <summary>
///     A single service ("banner") discovered on a host — one entry of a host's <c>data[]</c> array
///     and one element of a search result's <c>matches[]</c>. Strongly-typed for the common fields
///     and the heavily-used <c>http</c>/<c>ssl</c> modules; other protocol modules are available on
///     the raw wire payload.
/// </summary>
public sealed record Banner
{
	/// <summary>Textual IP address (<c>ip_str</c>).</summary>
	public required string IpString { get; init; }

	/// <summary>Integer form of the IPv4 address, if applicable (<c>ip</c>).</summary>
	public long? Ip { get; init; }

	/// <summary>Textual IPv6 address, if applicable (<c>ipv6</c>).</summary>
	public string? IpV6 { get; init; }

	/// <summary>The port the service runs on (<c>port</c>).</summary>
	public int Port { get; init; }

	/// <summary>Transport protocol, <c>tcp</c> or <c>udp</c> (<c>transport</c>).</summary>
	public string? Transport { get; init; }

	/// <summary>Application protocol name where distinct from the module (<c>protocol</c>).</summary>
	public string? Protocol { get; init; }

	/// <summary>Raw banner text captured from the service (<c>data</c>).</summary>
	public string? Data { get; init; }

	/// <summary>Identified product name (<c>product</c>).</summary>
	public string? Product { get; init; }

	/// <summary>Identified product version (<c>version</c>).</summary>
	public string? Version { get; init; }

	/// <summary>Additional descriptive information (<c>info</c>).</summary>
	public string? Info { get; init; }

	/// <summary>Operating system, if fingerprinted (<c>os</c>).</summary>
	public string? OperatingSystem { get; init; }

	/// <summary>CPE 2.2 identifiers (<c>cpe</c>).</summary>
	public IReadOnlyList<Cpe> Cpe { get; init; } = [];

	/// <summary>CPE 2.3 identifiers (<c>cpe23</c>).</summary>
	public IReadOnlyList<Cpe> Cpe23 { get; init; } = [];

	/// <summary>Hostnames associated with the service (<c>hostnames</c>).</summary>
	public IReadOnlyList<string> Hostnames { get; init; } = [];

	/// <summary>Registered domains associated with the service (<c>domains</c>).</summary>
	public IReadOnlyList<string> Domains { get; init; } = [];

	/// <summary>Classification tags (<c>tags</c>).</summary>
	public IReadOnlyList<string> Tags { get; init; } = [];

	/// <summary>Autonomous System Number, e.g. <c>AS15169</c> (<c>asn</c>).</summary>
	public string? Asn { get; init; }

	/// <summary>Internet Service Provider (<c>isp</c>).</summary>
	public string? Isp { get; init; }

	/// <summary>Owning organization (<c>org</c>).</summary>
	public string? Organization { get; init; }

	/// <summary>When the banner was collected (<c>timestamp</c>).</summary>
	public DateTimeOffset? Timestamp { get; init; }

	/// <summary>Shodan's hash of the banner text (<c>hash</c>).</summary>
	public long? Hash { get; init; }

	/// <summary>Geographic location (<c>location</c>).</summary>
	public GeoLocation? Location { get; init; }

	/// <summary>Collection metadata (<c>_shodan</c>).</summary>
	public BannerShodanMetadata? Metadata { get; init; }

	/// <summary>Vulnerabilities keyed by CVE identifier (<c>vulns</c>).</summary>
	public IReadOnlyDictionary<string, Vulnerability> Vulnerabilities { get; init; } =
		new Dictionary<string, Vulnerability>();

	/// <summary>Details of the <c>http</c> module, when present.</summary>
	public HttpBanner? Http { get; init; }

	/// <summary>Details of the <c>ssl</c> module, when present.</summary>
	public SslBanner? Ssl { get; init; }

	/// <summary>Details of the <c>dns</c> module, when present.</summary>
	public BannerDnsModule? Dns { get; init; }
}
