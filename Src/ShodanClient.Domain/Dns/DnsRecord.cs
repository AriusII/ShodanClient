namespace ShodanClient.Domain.Dns;

/// <summary>
///     A single DNS record entry for a domain, as returned by <c>GET /dns/domain/{domain}</c>.
/// </summary>
public sealed record DnsRecord
{
	/// <summary>The subdomain the record applies to, or an empty string for the domain apex (<c>subdomain</c>).</summary>
	public required string Subdomain { get; init; }

	/// <summary>
	///     The DNS record type, e.g. <c>A</c>, <c>AAAA</c>, <c>MX</c>, <c>NS</c>, <c>TXT</c>, <c>CNAME</c>,
	///     <c>SOA</c> (<c>type</c>).
	/// </summary>
	public required string Type { get; init; }

	/// <summary>The record's value (<c>value</c>).</summary>
	public required string Value { get; init; }

	/// <summary>When the record was last seen (<c>last_seen</c>).</summary>
	public DateTimeOffset? LastSeen { get; init; }

	/// <summary>Ports observed alongside this record, when available (<c>ports</c>).</summary>
	public IReadOnlyList<int> Ports { get; init; } = [];
}
