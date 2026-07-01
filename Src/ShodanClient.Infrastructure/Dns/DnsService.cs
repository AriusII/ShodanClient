using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Abstractions.Services;
using ShodanClient.Application.Common;
using ShodanClient.Application.Dns;
using ShodanClient.Domain.Dns;

namespace ShodanClient.Infrastructure.Dns;

/// <summary>
///     Logic layer for DNS lookups against Shodan. Validates inputs, joins collections into
///     comma-separated parameters and delegates to the repository.
/// </summary>
internal sealed class DnsService(IDnsRepository repository) : IDnsService
{
	public Task<DomainDnsInfo> GetDomainAsync(
		string domain,
		bool history = false,
		string? type = null,
		int page = 1,
		CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(domain);
		return repository.GetDomainAsync(new DomainDnsQuery(domain, history, type, page), cancellationToken);
	}

	public Task<IReadOnlyDictionary<string, string?>> ResolveAsync(
		IEnumerable<string> hostnames,
		CancellationToken cancellationToken = default)
	{
		var targets = Guard.NotNullOrEmpty(hostnames as IReadOnlyList<string> ?? hostnames?.ToList());
		var csv = string.Join(',', targets);
		return repository.ResolveAsync(new DnsResolveQuery(csv), cancellationToken);
	}

	public Task<IReadOnlyDictionary<string, IReadOnlyList<string>>> ReverseAsync(
		IEnumerable<string> ips,
		CancellationToken cancellationToken = default)
	{
		var targets = Guard.NotNullOrEmpty(ips as IReadOnlyList<string> ?? ips?.ToList());
		var csv = string.Join(',', targets);
		return repository.ReverseAsync(new DnsReverseQuery(csv), cancellationToken);
	}
}
