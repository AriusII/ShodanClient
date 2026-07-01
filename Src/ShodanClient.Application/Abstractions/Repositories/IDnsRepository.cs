using ShodanClient.Application.Dns;
using ShodanClient.Domain.Dns;

namespace ShodanClient.Application.Abstractions.Repositories;

/// <summary>Transport-level access to the DNS family of endpoints on the REST API.</summary>
internal interface IDnsRepository
{
	/// <summary>Looks up subdomains and DNS entries for a domain (<c>GET /dns/domain/{domain}</c>).</summary>
	Task<DomainDnsInfo> GetDomainAsync(DomainDnsQuery query, CancellationToken cancellationToken);

	/// <summary>Resolves hostnames to their IP addresses (<c>GET /dns/resolve</c>).</summary>
	Task<IReadOnlyDictionary<string, string?>> ResolveAsync(DnsResolveQuery query, CancellationToken cancellationToken);

	/// <summary>Resolves IP addresses to their hostnames (<c>GET /dns/reverse</c>).</summary>
	Task<IReadOnlyDictionary<string, IReadOnlyList<string>>> ReverseAsync(
		DnsReverseQuery query,
		CancellationToken cancellationToken);
}
