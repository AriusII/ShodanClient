using ShodanClient.Application.Common;

namespace ShodanClient.Application.Search;

/// <summary>Parameters for a host lookup (<c>GET /shodan/host/{ip}</c>).</summary>
/// <param name="Ip">The host IP address.</param>
/// <param name="History">Whether to include all historical banners.</param>
/// <param name="Minify">Whether to return only ports and general host information (no banners).</param>
internal sealed record HostLookupQuery(string Ip, bool History = false, bool Minify = false) : IShodanQuery;
