namespace ShodanClient.Domain.Dns;

/// <summary>
///     Subdomains and DNS entries collected for a domain (<c>GET /dns/domain/{domain}</c>).
/// </summary>
public sealed record DomainDnsInfo
{
	/// <summary>The domain that was looked up (<c>domain</c>).</summary>
	public required string Domain { get; init; }

	/// <summary>Classification tags (<c>tags</c>).</summary>
	public IReadOnlyList<string> Tags { get; init; } = [];

	/// <summary>Known subdomains (<c>subdomains</c>).</summary>
	public IReadOnlyList<string> Subdomains { get; init; } = [];

	/// <summary>Whether more DNS entries exist beyond this page (<c>more</c>).</summary>
	public bool More { get; init; }

	/// <summary>The individual DNS records collected for the domain (<c>data</c>).</summary>
	public IReadOnlyList<DnsRecord> Data { get; init; } = [];
}
