namespace ShodanClient.Domain.Search;

/// <summary>
///     The <c>dns</c> block present on banners collected by Shodan's DNS module (e.g. resolver
///     probes and some alert-stream banners), describing the resolver that answered the query.
/// </summary>
public sealed record BannerDnsModule
{
	/// <summary>Hostname of the resolver that answered the query (<c>dns.resolver_hostname</c>).</summary>
	public string? ResolverHostname { get; init; }

	/// <summary>Whether the resolver performs recursive lookups (<c>dns.recursive</c>).</summary>
	public bool? Recursive { get; init; }

	/// <summary>Identifier of the resolver (<c>dns.resolver_id</c>).</summary>
	public string? ResolverId { get; init; }

	/// <summary>Resolver software name/version, when fingerprinted (<c>dns.software</c>).</summary>
	public string? Software { get; init; }
}
