using ShodanClient.Application.Common;

namespace ShodanClient.Application.Dns;

/// <summary>Parameters for a domain DNS lookup (<c>GET /dns/domain/{domain}</c>).</summary>
/// <param name="Domain">The domain name to look up.</param>
/// <param name="History">Whether historical DNS data should be included.</param>
/// <param name="Type">The DNS record type to filter by, e.g. <c>A</c>, <c>MX</c>, <c>TXT</c>.</param>
/// <param name="Page">The page of results to fetch (100 results per page).</param>
internal sealed record DomainDnsQuery(string Domain, bool History = false, string? Type = null, int Page = 1)
	: IShodanQuery;
