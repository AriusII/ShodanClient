using System.Globalization;
using System.Security.Cryptography;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShodanClient.App.Services.Notifications;
using ShodanClient.App.Services.Profiles;
using ShodanClient.App.Services.Settings;
using ShodanClient.App.Session;
using AvaloniaApplication = Avalonia.Application;

namespace ShodanClient.App.ViewModels.Settings;

/// <summary>
///     Local app preferences: profile (API key) management, theme, resilience/rate-limit overrides.
///     The full-surface counterpart to the compact "Switch account" flyout in the shell's footer.
/// </summary>
public sealed partial class SettingsViewModel : ModuleViewModelBase
{
	/// <summary>Creates the Settings module view model.</summary>
	public SettingsViewModel(
		INotificationService notifications,
		IProfileService profiles,
		ISettingsService settingsService)
		: base(notifications)
	{
		Profiles = profiles;
		SettingsService = settingsService;
		Title = "Settings";
		MaxRetriesInput = settingsService.Current.Resilience?.MaxRetries?.ToString(CultureInfo.InvariantCulture) ??
						  string.Empty;
		PermitsPerSecondInput =
			settingsService.Current.RateLimit?.PermitsPerSecond?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

		// Syncs the live theme with whatever was last persisted (App.axaml itself always boots
		// dark; this is the first opportunity to correct that from a saved preference). Applied
		// unconditionally since the generated setter only invokes OnSelectedThemeChanged when the
		// value differs from the (unset) backing field's default.
		SelectedTheme = settingsService.Current.Theme;
		ApplyThemeVariant(SelectedTheme);
	}

	/// <summary>The saved profiles and the currently active one.</summary>
	public IProfileService Profiles { get; }

	/// <summary>The local settings store (theme, window size, resilience overrides).</summary>
	public ISettingsService SettingsService { get; }

	/// <summary>The selectable theme options, in display order.</summary>
	public IReadOnlyList<ThemePreference> ThemeOptions { get; } = Enum.GetValues<ThemePreference>();

	#region Profiles

	[ObservableProperty] public partial string NewProfileName { get; set; } = string.Empty;

	[ObservableProperty] public partial string NewProfileApiKey { get; set; } = string.Empty;

	[ObservableProperty] public partial string? AddProfileError { get; set; }

	[ObservableProperty] public partial Guid? RenamingProfileId { get; set; }

	[ObservableProperty] public partial string RenameInput { get; set; } = string.Empty;

	/// <summary>Validates and saves a brand-new profile from <see cref="NewProfileName" />/<see cref="NewProfileApiKey" />.</summary>
	[RelayCommand]
	private Task AddProfileAsync(CancellationToken cancellationToken) => RunAsync(async ct =>
	{
		AddProfileError = null;
		var apiKey = NewProfileApiKey.Trim();
		if (apiKey.Length == 0)
		{
			AddProfileError = "Enter an API key.";
			return;
		}

		var name = string.IsNullOrWhiteSpace(NewProfileName) ? "Profile" : NewProfileName.Trim();

		bool added;
		try
		{
			added = await Profiles.AddProfileAsync(name, apiKey, ct).ConfigureAwait(true);
		}
		catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or CryptographicException)
		{
			AddProfileError = "Could not save the profile to disk. Check permissions and try again.";
			return;
		}

		if (!added)
		{
			AddProfileError = "Shodan rejected this API key. Double-check it and try again.";
			return;
		}

		NewProfileName = string.Empty;
		NewProfileApiKey = string.Empty;
	}, cancellationToken);

	/// <summary>Attaches a previously saved profile's key and makes it active.</summary>
	[RelayCommand]
	private Task SwitchProfileAsync(ApiProfileDescriptor profile, CancellationToken cancellationToken) => RunAsync(
		async ct =>
		{
			AddProfileError = null;
			try
			{
				var switched = await Profiles.SwitchToAsync(profile.Id, ct).ConfigureAwait(true);
				if (!switched)
				{
					AddProfileError = $"Could not switch to '{profile.Name}'. The saved key may no longer be valid.";
				}
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or CryptographicException)
			{
				AddProfileError = "Could not read the saved profile from disk.";
			}
		}, cancellationToken);

	/// <summary>Deletes a saved profile, after the view's confirmation flyout has already asked.</summary>
	[RelayCommand]
	private Task RemoveProfileAsync(ApiProfileDescriptor profile, CancellationToken cancellationToken) => RunAsync(
		async ct =>
		{
			try
			{
				await Profiles.RemoveAsync(profile.Id, ct).ConfigureAwait(true);
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or CryptographicException)
			{
				AddProfileError = "Could not remove the profile from disk.";
			}
		}, cancellationToken);

	/// <summary>Begins inline-renaming a profile row.</summary>
	[RelayCommand]
	private void BeginRename(ApiProfileDescriptor profile)
	{
		RenamingProfileId = profile.Id;
		RenameInput = profile.Name;
	}

	/// <summary>Cancels an in-progress inline rename without saving.</summary>
	[RelayCommand]
	private void CancelRename() => RenamingProfileId = null;

	/// <summary>Commits <see cref="RenameInput" /> as the new name for <see cref="RenamingProfileId" />.</summary>
	[RelayCommand]
	private Task ConfirmRenameAsync(CancellationToken cancellationToken) => RunAsync(async ct =>
	{
		if (RenamingProfileId is not { } profileId)
		{
			return;
		}

		var newName = RenameInput.Trim();
		if (newName.Length == 0)
		{
			return;
		}

		try
		{
			await Profiles.RenameAsync(profileId, newName, ct).ConfigureAwait(true);
		}
		catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or CryptographicException)
		{
			AddProfileError = "Could not save the new name to disk.";
			return;
		}

		RenamingProfileId = null;
	}, cancellationToken);

	#endregion

	#region Appearance

	[ObservableProperty] public partial ThemePreference SelectedTheme { get; set; }

	partial void OnSelectedThemeChanged(ThemePreference value)
	{
		SettingsService.Current.Theme = value;
		ApplyThemeVariant(value);
		_ = PersistThemeAsync();
	}

	private static void ApplyThemeVariant(ThemePreference preference)
	{
		if (AvaloniaApplication.Current is not { } application)
		{
			return;
		}

		application.RequestedThemeVariant = preference switch
		{
			ThemePreference.Light => ThemeVariant.Light,
			ThemePreference.Dark => ThemeVariant.Dark,
			_ => ThemeVariant.Default
		};
	}

	private async Task PersistThemeAsync()
	{
		try
		{
			await SettingsService.SaveAsync().ConfigureAwait(true);
		}
		catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
		{
			// Best-effort: the live theme is already applied; a failed write just means the
			// choice won't survive a restart.
		}
	}

	#endregion

	#region Advanced

	[ObservableProperty] public partial string MaxRetriesInput { get; set; }

	[ObservableProperty] public partial string PermitsPerSecondInput { get; set; }

	[ObservableProperty] public partial string? AdvancedErrorMessage { get; set; }

	[ObservableProperty] public partial string? AdvancedStatusMessage { get; set; }

	/// <summary>Validates and persists the resilience/rate-limit overrides; they apply on the next connect.</summary>
	[RelayCommand]
	private Task SaveAdvancedAsync(CancellationToken cancellationToken) => RunAsync(async ct =>
	{
		AdvancedErrorMessage = null;
		AdvancedStatusMessage = null;

		int? maxRetries = null;
		if (!string.IsNullOrWhiteSpace(MaxRetriesInput))
		{
			if (!int.TryParse(MaxRetriesInput, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ||
				parsed < 0)
			{
				AdvancedErrorMessage = "Max retries must be a whole number of 0 or more.";
				return;
			}

			maxRetries = parsed;
		}

		int? permitsPerSecond = null;
		if (!string.IsNullOrWhiteSpace(PermitsPerSecondInput))
		{
			if (!int.TryParse(PermitsPerSecondInput, NumberStyles.Integer, CultureInfo.InvariantCulture,
					out var parsed) || parsed < 1)
			{
				AdvancedErrorMessage = "Permits per second must be a whole number of 1 or more.";
				return;
			}

			permitsPerSecond = parsed;
		}

		try
		{
			var settings = SettingsService.Current;
			settings.Resilience ??= new ResilienceOverride();
			settings.Resilience.MaxRetries = maxRetries;
			settings.RateLimit ??= new RateLimitOverride();
			settings.RateLimit.PermitsPerSecond = permitsPerSecond;

			await SettingsService.SaveAsync(ct).ConfigureAwait(true);
			AdvancedStatusMessage = "Saved. Applies the next time you connect.";
		}
		catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
		{
			AdvancedErrorMessage = "Could not save settings to disk.";
		}
	}, cancellationToken);

	#endregion
}
