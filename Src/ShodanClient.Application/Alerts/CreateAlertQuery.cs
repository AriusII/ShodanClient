using ShodanClient.Application.Common;

namespace ShodanClient.Application.Alerts;

/// <summary>Parameters for creating a network alert (<c>POST /shodan/alert</c>).</summary>
/// <param name="Name">A display name describing the alert.</param>
/// <param name="Ips">The IPs or network ranges (CIDR notation) to monitor.</param>
/// <param name="Expires">The number of seconds the alert stays active, or <c>0</c> to never expire.</param>
internal sealed record CreateAlertQuery(string Name, IReadOnlyList<string> Ips, int Expires) : IShodanQuery;
