using ShodanClient.Application.Common;

namespace ShodanClient.Application.Scanning;

/// <summary>
///     Parameters for requesting an on-demand crawl of specific IPs/netblocks (<c>POST /shodan/scan</c>).
/// </summary>
/// <param name="Ips">Comma-separated list of IP addresses or netblocks (CIDR) to scan.</param>
internal sealed record ScanRequestQuery(string Ips) : IShodanQuery;
