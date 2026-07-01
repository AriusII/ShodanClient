using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ShodanClient.App.Services.AccountSession;
using ShodanClient.App.Services.Credentials;
using ShodanClient.App.Services.Settings;
using ShodanClient.App.Services.ShodanClientAccessor;
using ShodanClient.App.Session;

namespace ShodanClient.App.Services.Profiles;

/// <summary>Default <see cref="IProfileService" />.</summary>
/// <remarks>Creates the profile service.</remarks>
public sealed partial class ProfileService(
	ICredentialStore credentialStore,
	IShodanClientAccessor accessor,
	IAccountSessionService accountSession,
	ISettingsService settingsService) : ObservableObject, IProfileService
{
	[ObservableProperty] public partial ApiProfileDescriptor? ActiveProfile { get; private set; }

	/// <inheritdoc />
	public ObservableCollection<ApiProfileDescriptor> Profiles { get; } = [];

	/// <inheritdoc />
	public event Action? Changed;

	/// <inheritdoc />
	public async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		await ReloadProfilesAsync(cancellationToken).ConfigureAwait(true);

		var activeId = await credentialStore.GetActiveProfileIdAsync(cancellationToken).ConfigureAwait(true);
		if (activeId is { } id && Profiles.FirstOrDefault(p => p.Id == id) is { } descriptor)
		{
			var apiKey = await credentialStore.TryGetApiKeyAsync(id, cancellationToken).ConfigureAwait(true);
			if (!string.IsNullOrWhiteSpace(apiKey))
			{
				var attached = await accessor.TryAttachAsync(apiKey, settingsService.Current, cancellationToken)
					.ConfigureAwait(true);
				if (attached)
				{
					ActiveProfile = descriptor;
				}
			}
		}

		Changed?.Invoke();
	}

	/// <inheritdoc />
	public async Task<bool> AddProfileAsync(string name, string apiKey, CancellationToken cancellationToken = default)
	{
		var trimmedName = string.IsNullOrWhiteSpace(name) ? "Profile" : name.Trim();
		var trimmedKey = apiKey.Trim();
		if (trimmedKey.Length == 0)
		{
			return false;
		}

		var attached = await accessor.TryAttachAsync(trimmedKey, settingsService.Current, cancellationToken)
			.ConfigureAwait(true);
		if (!attached)
		{
			return false;
		}

		var descriptor = await credentialStore.AddProfileAsync(trimmedName, trimmedKey, cancellationToken)
			.ConfigureAwait(true);
		await credentialStore.SetActiveProfileAsync(descriptor.Id, cancellationToken).ConfigureAwait(true);

		await ReloadProfilesAsync(cancellationToken).ConfigureAwait(true);
		ActiveProfile = Profiles.FirstOrDefault(p => p.Id == descriptor.Id);

		await accountSession.RefreshAsync(true, cancellationToken).ConfigureAwait(true);
		Changed?.Invoke();
		return true;
	}

	/// <inheritdoc />
	public async Task<bool> SwitchToAsync(Guid profileId, CancellationToken cancellationToken = default)
	{
		var apiKey = await credentialStore.TryGetApiKeyAsync(profileId, cancellationToken).ConfigureAwait(true);
		if (string.IsNullOrWhiteSpace(apiKey))
		{
			return false;
		}

		var attached = await accessor.TryAttachAsync(apiKey, settingsService.Current, cancellationToken)
			.ConfigureAwait(true);
		if (!attached)
		{
			return false;
		}

		await credentialStore.SetActiveProfileAsync(profileId, cancellationToken).ConfigureAwait(true);
		await ReloadProfilesAsync(cancellationToken).ConfigureAwait(true);
		ActiveProfile = Profiles.FirstOrDefault(p => p.Id == profileId);

		await accountSession.RefreshAsync(true, cancellationToken).ConfigureAwait(true);
		Changed?.Invoke();
		return true;
	}

	/// <inheritdoc />
	public async Task RemoveAsync(Guid profileId, CancellationToken cancellationToken = default)
	{
		var wasActive = ActiveProfile?.Id == profileId;
		await credentialStore.RemoveProfileAsync(profileId, cancellationToken).ConfigureAwait(true);
		await ReloadProfilesAsync(cancellationToken).ConfigureAwait(true);

		if (!wasActive)
		{
			Changed?.Invoke();
			return;
		}

		var next = Profiles.FirstOrDefault();
		if (next is not null)
		{
			// Reloads profiles and raises Changed again; harmless, and keeps SwitchToAsync as the
			// single place that knows how to attach + refresh the account session.
			var switched = await SwitchToAsync(next.Id, cancellationToken).ConfigureAwait(true);
			if (switched)
			{
				return;
			}
		}

		// Either no profiles remain, or the fallback candidate's saved key no longer works: detach
		// entirely rather than leaving ActiveProfile pointing at the profile that was just removed.
		ActiveProfile = null;
		await accessor.DetachAsync().ConfigureAwait(true);
		Changed?.Invoke();
	}

	/// <inheritdoc />
	public async Task RenameAsync(Guid profileId, string newName, CancellationToken cancellationToken = default)
	{
		var trimmed = newName.Trim();
		if (trimmed.Length == 0)
		{
			return;
		}

		await credentialStore.RenameProfileAsync(profileId, trimmed, cancellationToken).ConfigureAwait(true);
		await ReloadProfilesAsync(cancellationToken).ConfigureAwait(true);

		if (ActiveProfile?.Id == profileId)
		{
			ActiveProfile = Profiles.FirstOrDefault(p => p.Id == profileId);
		}

		Changed?.Invoke();
	}

	private async Task ReloadProfilesAsync(CancellationToken cancellationToken)
	{
		var profiles = await credentialStore.GetProfilesAsync(cancellationToken).ConfigureAwait(true);
		Profiles.Clear();
		foreach (var profile in profiles.OrderBy(p => p.CreatedAt))
		{
			Profiles.Add(profile);
		}
	}
}
