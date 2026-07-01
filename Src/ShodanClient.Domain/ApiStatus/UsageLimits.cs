namespace ShodanClient.Domain.ApiStatus;

/// <summary>
///     The plan-level ceilings reported alongside <see cref="ApiInfo" /> (<c>api-info.usage_limits</c>).
/// </summary>
public sealed record UsageLimits
{
	/// <summary>Maximum scan credits allowed by the plan; <c>-1</c> means unlimited (<c>scan_credits</c>).</summary>
	public required int ScanCredits { get; init; }

	/// <summary>Maximum query credits allowed by the plan (<c>query_credits</c>).</summary>
	public required int QueryCredits { get; init; }

	/// <summary>Maximum number of IPs that can be monitored, if applicable (<c>monitored_ips</c>).</summary>
	public int? MonitoredIps { get; init; }
}
