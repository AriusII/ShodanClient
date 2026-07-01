namespace ShodanClient.App.Services.Credentials;

/// <summary>The JSON payload encrypted at rest by <see cref="DpapiCredentialStore" />.</summary>
public sealed class CredentialPayload
{
	/// <summary>Every saved profile.</summary>
	public List<StoredProfile> Profiles { get; init; } = [];

	/// <summary>The id of the profile that should reattach automatically on the next launch, if any.</summary>
	public Guid? ActiveProfileId { get; set; }
}
