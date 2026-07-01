namespace ShodanClient.App.Services.Credentials;

/// <summary>A single saved profile as persisted inside <see cref="CredentialPayload" />.</summary>
public sealed record StoredProfile
{
	/// <summary>A stable identifier for this profile, independent of its display name.</summary>
	public required Guid Id { get; init; }

	/// <summary>The user-chosen display name.</summary>
	public required string Name { get; init; }

	/// <summary>The raw Shodan API key. Never leaves the encrypted store as plain text.</summary>
	public required string ApiKey { get; init; }

	/// <summary>When this profile was first added.</summary>
	public required DateTimeOffset CreatedAt { get; init; }
}
