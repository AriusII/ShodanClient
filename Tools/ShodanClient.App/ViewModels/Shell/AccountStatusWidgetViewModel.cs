using System.Security.Cryptography;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShodanClient.App.Services.AccountSession;
using ShodanClient.App.Services.Profiles;
using ShodanClient.App.Session;

namespace ShodanClient.App.ViewModels.Shell;

/// <summary>
///     Backs <c>AccountStatusWidget</c>: the linked account's plan/credit summary plus the compact
///     "Switch account" flyout's profile list, switch/remove actions and inline add-profile
///     mini-form. The fuller, power-user counterpart of this same data lives in Settings.
/// </summary>
/// <remarks>Creates the widget view model.</remarks>
public sealed partial class AccountStatusWidgetViewModel(
	IAccountSessionService account,
	IProfileService profiles,
	bool isBusy) : ObservableObject
{
	[ObservableProperty] public partial string? AddProfileError { get; set; }

	[ObservableProperty] public partial bool IsBusy { get; set; } = isBusy;

	[ObservableProperty] public partial string NewProfileApiKey { get; set; } = string.Empty;

	[ObservableProperty] public partial string NewProfileName { get; set; } = string.Empty;

	/// <summary>The account/plan tracking service, bound to by the widget's summary section.</summary>
	public IAccountSessionService Account { get; } = account;

	/// <summary>The saved profiles and the currently active one, bound to by the "Switch account" flyout.</summary>
	public IProfileService Profiles { get; } = profiles;

	/// <summary>Force-refreshes the account/plan summary.</summary>
	[RelayCommand]
	private Task RefreshAsync() => Account.RefreshAsync(true);

	/// <summary>Attaches a previously saved profile's key and makes it active.</summary>
	[RelayCommand]
	private async Task SwitchProfileAsync(ApiProfileDescriptor profile)
	{
		IsBusy = true;
		AddProfileError = null;
		try
		{
			var switched = await Profiles.SwitchToAsync(profile.Id).ConfigureAwait(true);
			if (!switched)
			{
				AddProfileError = $"Could not switch to '{profile.Name}'. The saved key may no longer be valid.";
			}
		}
		catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or CryptographicException)
		{
			AddProfileError = "Could not read the saved profile from disk.";
		}
		finally
		{
			IsBusy = false;
		}
	}

	/// <summary>Validates and saves a brand-new profile from <see cref="NewProfileName" />/<see cref="NewProfileApiKey" />.</summary>
	[RelayCommand]
	private async Task AddProfileAsync()
	{
		AddProfileError = null;
		var apiKey = NewProfileApiKey.Trim();
		if (apiKey.Length == 0)
		{
			AddProfileError = "Enter an API key.";
			return;
		}

		var name = string.IsNullOrWhiteSpace(NewProfileName) ? "Profile" : NewProfileName.Trim();

		IsBusy = true;
		try
		{
			bool added;
			try
			{
				added = await Profiles.AddProfileAsync(name, apiKey).ConfigureAwait(true);
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
		}
		finally
		{
			IsBusy = false;
		}
	}

	/// <summary>Deletes a saved profile, after the view's confirmation flyout has already asked.</summary>
	[RelayCommand]
	private async Task RemoveProfileAsync(ApiProfileDescriptor profile)
	{
		IsBusy = true;
		try
		{
			await Profiles.RemoveAsync(profile.Id).ConfigureAwait(true);
		}
		catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or CryptographicException)
		{
			AddProfileError = "Could not remove the profile from disk.";
		}
		finally
		{
			IsBusy = false;
		}
	}
}
