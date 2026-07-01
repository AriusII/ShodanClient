using System.Collections.ObjectModel;
using ShodanClient.App.Session;

namespace ShodanClient.App.Services.Profiles;

/// <summary>
///     Manages the set of saved Shodan API key profiles and which one is active, wiring every switch
///     through to <see cref="Services.ShodanClientAccessor.IShodanClientAccessor" /> and
///     <see cref="Services.AccountSession.IAccountSessionService" /> so the rest of the app never talks
///     to <see cref="Credentials.ICredentialStore" /> directly.
/// </summary>
public interface IProfileService
{
	/// <summary>Every saved profile, in creation order.</summary>
	ObservableCollection<ApiProfileDescriptor> Profiles { get; }

	/// <summary>The profile currently attached to <see cref="Services.ShodanClientAccessor.IShodanClientAccessor" />, if any.</summary>
	ApiProfileDescriptor? ActiveProfile { get; }

	/// <summary>Raised whenever <see cref="Profiles" /> or <see cref="ActiveProfile" /> changes.</summary>
	event Action? Changed;

	/// <summary>
	///     Loads every saved profile and, if one was previously active, attaches it. Called once at
	///     shell startup in place of the app's old ad-hoc single-key bootstrap.
	/// </summary>
	Task InitializeAsync(CancellationToken cancellationToken = default);

	/// <summary>
	///     Validates <paramref name="apiKey" /> for real against Shodan and, only on success, saves it
	///     as a new profile, makes it active and force-refreshes the account session.
	/// </summary>
	Task<bool> AddProfileAsync(string name, string apiKey, CancellationToken cancellationToken = default);

	/// <summary>Attaches the saved key for <paramref name="profileId" /> and, on success, makes it active.</summary>
	Task<bool> SwitchToAsync(Guid profileId, CancellationToken cancellationToken = default);

	/// <summary>
	///     Deletes a saved profile. If it was active and others remain, automatically switches to
	///     another one; if none remain, detaches the client entirely (falls back to the Setup gate).
	/// </summary>
	Task RemoveAsync(Guid profileId, CancellationToken cancellationToken = default);

	/// <summary>Renames a saved profile.</summary>
	Task RenameAsync(Guid profileId, string newName, CancellationToken cancellationToken = default);
}
