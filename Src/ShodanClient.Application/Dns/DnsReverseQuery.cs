using ShodanClient.Application.Common;

namespace ShodanClient.Application.Dns;

/// <summary>Parameters for an IP-to-hostname reverse resolution (<c>GET /dns/reverse</c>).</summary>
/// <param name="IpsCsv">Comma-separated IP addresses to reverse-resolve.</param>
internal sealed record DnsReverseQuery(string IpsCsv) : IShodanQuery;
