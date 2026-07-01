namespace ShodanClient.Domain.Account;

/// <summary>
///     The calling API key's account profile, as returned by <c>GET /account/profile</c>.
/// </summary>
public sealed record AccountProfile
{
	/// <summary>Whether the account has a paid membership (<c>member</c>).</summary>
	public required bool Member { get; init; }

	/// <summary>The number of query credits remaining (<c>credits</c>).</summary>
	public required int Credits { get; init; }

	/// <summary>The display name associated with the account, if set (<c>display_name</c>).</summary>
	public string? DisplayName { get; init; }

	/// <summary>When the account was created (<c>created</c>).</summary>
	public DateTimeOffset? Created { get; init; }
}
