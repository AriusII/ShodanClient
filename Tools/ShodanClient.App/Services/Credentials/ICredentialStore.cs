using ShodanClient.App.Session;

namespace ShodanClient.App.Services.Credentials;

/// <summary>
///     Persists every saved Shodan API key profile on disk, encrypted for the current Windows user.
///     Callers never see a raw key outside <see cref="TryGetApiKeyAsync" />; everywhere else a
///     profile is represented by its <see cref="ApiProfileDescriptor" /> (name + masked key).
/// </summary>
public interface ICredentialStore
{
	/// <summary>All saved profiles, in no particular guaranteed order.</summary>
	Task<IReadOnlyList<ApiProfileDescriptor>> GetProfilesAsync(CancellationToken cancellationToken = default);

	/// <summary>The id of the profile that was active the last time one was set, if any.</summary>
	Task<Guid?> GetActiveProfileIdAsync(CancellationToken cancellationToken = default);

	/// <summary>Attempts to read the raw API key for <paramref name="profileId" />, if it still exists.</summary>
	Task<string?> TryGetApiKeyAsync(Guid profileId, CancellationToken cancellationToken = default);

	/// <summary>Encrypts and saves a new profile, returning its descriptor. Does not mark it active.</summary>
	Task<ApiProfileDescriptor> AddProfileAsync(string name, string apiKey,
		CancellationToken cancellationToken = default);

	/// <summary>Deletes a saved profile. If it was the active profile, clears the active marker too.</summary>
	Task RemoveProfileAsync(Guid profileId, CancellationToken cancellationToken = default);

	/// <summary>Renames a saved profile in place.</summary>
	Task RenameProfileAsync(Guid profileId, string newName, CancellationToken cancellationToken = default);

	/// <summary>Marks <paramref name="profileId" /> as the active profile.</summary>
	Task SetActiveProfileAsync(Guid profileId, CancellationToken cancellationToken = default);
}
