namespace ShodanClient.Domain.ApiStatus;

/// <summary>
///     Account plan and credit status returned by <c>GET /api-info</c>.
/// </summary>
public sealed record ApiInfo
{
	/// <summary>Remaining on-demand scan credits (<c>scan_credits</c>).</summary>
	public required int ScanCredits { get; init; }

	/// <summary>Remaining query credits (<c>query_credits</c>).</summary>
	public required int QueryCredits { get; init; }

	/// <summary>Number of IPs currently under network monitoring, if applicable (<c>monitored_ips</c>).</summary>
	public int? MonitoredIps { get; init; }

	/// <summary>The account's plan name, e.g. <c>dev</c>, <c>oss</c>, <c>edu</c> (<c>plan</c>).</summary>
	public required string Plan { get; init; }

	/// <summary>Whether the account can access HTTPS-only search results (<c>https</c>).</summary>
	public required bool Https { get; init; }

	/// <summary>Whether the account can access Telnet-only search results (<c>telnet</c>).</summary>
	public required bool Telnet { get; init; }

	/// <summary>Whether the account has unlocked access to the full result set (<c>unlocked</c>).</summary>
	public required bool Unlocked { get; init; }

	/// <summary>Remaining unlocks for the current period (<c>unlocked_left</c>).</summary>
	public required int UnlockedLeft { get; init; }

	/// <summary>The plan's overall usage ceilings (<c>usage_limits</c>).</summary>
	public required UsageLimits UsageLimits { get; init; }
}
