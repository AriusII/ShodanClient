using ShodanClient.Domain.ApiStatus;

namespace ShodanClient.App.Session;

/// <summary>
///     A merged view of <c>Account.GetProfileAsync()</c> and <c>ApiInfo.GetAsync()</c>, refreshed by
///     <see cref="ShodanClient.App.Services.AccountSession.IAccountSessionService" /> and bound to by
///     <c>AccountStatusWidget</c>.
/// </summary>
public sealed record AccountSnapshot
{
	/// <summary>Whether the account has a paid membership.</summary>
	public required bool Member { get; init; }

	/// <summary>Remaining query credits.</summary>
	public required int Credits { get; init; }

	/// <summary>The display name associated with the account, if set.</summary>
	public string? DisplayName { get; init; }

	/// <summary>The account's plan name, e.g. <c>dev</c>, <c>oss</c>, <c>edu</c>.</summary>
	public required string Plan { get; init; }

	/// <summary>Remaining on-demand scan credits.</summary>
	public required int ScanCredits { get; init; }

	/// <summary>Remaining query credits as reported by <c>api-info</c> (mirrors <see cref="Credits" />).</summary>
	public required int QueryCredits { get; init; }

	/// <summary>Whether the account can access HTTPS-only search results.</summary>
	public required bool Https { get; init; }

	/// <summary>Whether the account can access Telnet-only search results.</summary>
	public required bool Telnet { get; init; }

	/// <summary>The plan's overall usage ceilings.</summary>
	public required UsageLimits UsageLimits { get; init; }
}
