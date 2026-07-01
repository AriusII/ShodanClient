using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Dns;
using ShodanClient.Domain.Dns;
using ShodanClient.Infrastructure.Dns.Mapping;
using ShodanClient.Infrastructure.Http;
using ShodanClient.Infrastructure.Http.Routing;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Infrastructure.Dns;

/// <summary>REST implementation of <see cref="IDnsRepository" />.</summary>
internal sealed class DnsRepository(RestChannel channel) : IDnsRepository
{
	public async Task<DomainDnsInfo> GetDomainAsync(DomainDnsQuery query, CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Dns.Domain(query.Domain, query.History, query.Type, query.Page);
		var response = await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.DomainDnsResponse, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}

	public async Task<IReadOnlyDictionary<string, string?>> ResolveAsync(
		DnsResolveQuery query,
		CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Dns.Resolve(query.HostnamesCsv);
		return await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.DictionaryStringString, cancellationToken)
			.ConfigureAwait(false);
	}

	public async Task<IReadOnlyDictionary<string, IReadOnlyList<string>>> ReverseAsync(
		DnsReverseQuery query,
		CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Dns.Reverse(query.IpsCsv);
		var response = await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.DictionaryStringStringArray, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}
}
