using ShodanClient.Domain.Dns;

namespace ShodanClient.Application.Abstractions.Services;

/// <summary>
///     DNS lookups against Shodan's DNS database. Exposed on the client as <c>IShodanClient.Dns</c>.
/// </summary>
public interface IDnsService
{
	/// <summary>
	///     Looks up subdomains and DNS entries collected for a domain (<c>GET /dns/domain/{domain}</c>).
	///     Consumes 1 query credit.
	/// </summary>
	/// <param name="domain">The domain name to look up, e.g. <c>example.com</c>.</param>
	/// <param name="history">Whether historical DNS data should be included. Defaults to <see langword="false" />.</param>
	/// <param name="type">
	///     An optional DNS record type filter, e.g. <c>A</c>, <c>AAAA</c>, <c>CNAME</c>, <c>NS</c>, <c>SOA</c>,
	///     <c>MX</c>, <c>TXT</c>.
	/// </param>
	/// <param name="page">The page of results to fetch, 100 results per page. Defaults to <c>1</c>.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<DomainDnsInfo> GetDomainAsync(
		string domain,
		bool history = false,
		string? type = null,
		int page = 1,
		CancellationToken cancellationToken = default);

	/// <summary>
	///     Resolves hostnames to their IP addresses (<c>GET /dns/resolve</c>). A hostname that failed to
	///     resolve is present in the result with a <see langword="null" /> value.
	/// </summary>
	/// <param name="hostnames">The hostnames to resolve.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<IReadOnlyDictionary<string, string?>> ResolveAsync(
		IEnumerable<string> hostnames,
		CancellationToken cancellationToken = default);

	/// <summary>
	///     Resolves IP addresses to their hostnames (<c>GET /dns/reverse</c>). An IP with no PTR record
	///     is present in the result with an empty list.
	/// </summary>
	/// <param name="ips">The IP addresses to reverse-resolve.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<IReadOnlyDictionary<string, IReadOnlyList<string>>> ReverseAsync(
		IEnumerable<string> ips,
		CancellationToken cancellationToken = default);
}
