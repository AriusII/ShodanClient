using ShodanClient.Application.Common;

namespace ShodanClient.Application.Alerts;

/// <summary>Parameters for replacing the monitored networks on an alert (<c>POST /shodan/alert/{id}</c>).</summary>
/// <param name="Id">The alert identifier.</param>
/// <param name="Ips">The new set of IPs or network ranges (CIDR notation) to monitor.</param>
internal sealed record UpdateAlertNetworksQuery(string Id, IReadOnlyList<string> Ips) : IShodanQuery;
