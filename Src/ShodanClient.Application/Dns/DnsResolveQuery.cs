using ShodanClient.Application.Common;

namespace ShodanClient.Application.Dns;

/// <summary>Parameters for a hostname-to-IP resolution (<c>GET /dns/resolve</c>).</summary>
/// <param name="HostnamesCsv">Comma-separated hostnames to resolve.</param>
internal sealed record DnsResolveQuery(string HostnamesCsv) : IShodanQuery;
