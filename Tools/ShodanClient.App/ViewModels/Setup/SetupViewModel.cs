using System.Security.Cryptography;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShodanClient.App.Services.Navigation;
using ShodanClient.App.Services.Notifications;
using ShodanClient.App.Services.Profiles;
using ShodanClient.App.Services.ShodanClientAccessor;
using ShodanClient.Application.Exceptions;
using ShodanClient.Domain.InternetDb;

namespace ShodanClient.App.ViewModels.Setup;

/// <summary>
///     First-run/invalid-key onboarding gate. Validates and saves the API key without ever routing
///     through <see cref="INotificationService" />: a failed first-run key is a form validation
///     error, not a global toast. Also hosts a free, key-less InternetDB preview lookup so a
///     brand-new user can see what Shodan can already tell them before ever entering a key.
/// </summary>
public sealed partial class SetupViewModel : ModuleViewModelBase
{
	private readonly IShodanClientAccessor _accessor;
	private readonly INavigationService _navigation;
	private readonly IProfileService _profiles;

	/// <summary>Creates the Setup module view model.</summary>
	public SetupViewModel(
		INotificationService notifications,
		IProfileService profiles,
		IShodanClientAccessor accessor,
		INavigationService navigation)
		: base(notifications)
	{
		_profiles = profiles;
		_accessor = accessor;
		_navigation = navigation;
		Title = "Setup";
	}

	[ObservableProperty] public partial string ApiKeyInput { get; set; } = string.Empty;

	[ObservableProperty] public partial string? ErrorMessage { get; set; }

	[ObservableProperty] public partial string? PreviewError { get; set; }

	[ObservableProperty] public partial string PreviewIpInput { get; set; } = string.Empty;

	[ObservableProperty] public partial InternetDbHost? PreviewResult { get; set; }

	/// <summary>Validates <see cref="ApiKeyInput" /> against Shodan and, on success, saves and attaches it.</summary>
	[RelayCommand]
	private Task ValidateAndSaveAsync(CancellationToken cancellationToken) => RunAsync(async ct =>
	{
		ErrorMessage = null;
		var apiKey = ApiKeyInput.Trim();
		if (apiKey.Length == 0)
		{
			ErrorMessage = "Enter your Shodan API key.";
			return;
		}

		bool added;
		try
		{
			added = await _profiles.AddProfileAsync("Default", apiKey, ct).ConfigureAwait(true);
		}
		catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or CryptographicException)
		{
			ErrorMessage = "Could not save the API key to disk. Check permissions and try again.";
			return;
		}

		if (!added)
		{
			ErrorMessage = "Shodan rejected this API key. Double-check it and try again.";
			return;
		}

		_navigation.NavigateTo(ModuleKey.Dashboard);
	}, cancellationToken);

	/// <summary>
	///     Looks up <see cref="PreviewIpInput" /> against the free InternetDB dataset, without needing
	///     any saved/validated API key.
	/// </summary>
	[RelayCommand]
	private Task PreviewLookupAsync(CancellationToken cancellationToken)
	{
		PreviewError = null;
		var ip = PreviewIpInput.Trim();
		if (ip.Length != 0)
		{
			return RunAsync(async ct =>
			{
				try
				{
					var client = await _accessor.GetAnonymousClientAsync(ct).ConfigureAwait(true);
					PreviewResult = await client.InternetDb.GetAsync(ip, ct).ConfigureAwait(true);
				}
				catch (ShodanRequestValidationException ex)
				{
					// Caught locally (rather than left to the base OnValidationError override) since this
					// screen has two independent forms — the API key box and this preview box — each with
					// its own error slot.
					PreviewError = ex.Message;
				}
			}, cancellationToken);
		}

		PreviewError = "Enter an IP address to look up.";
		return Task.CompletedTask;
	}

	/// <summary>
	///     Navigates to Settings. The shell hides its nav pane while Setup is active (so first-run
	///     onboarding gets a focused, full-screen presentation instead of a mostly-disabled sidebar),
	///     which would otherwise leave no way back to Settings if e.g. the active profile's key was
	///     revoked and other saved profiles still work — this link is that escape hatch.
	/// </summary>
	[RelayCommand]
	private void GoToSettings() => _navigation.NavigateTo(ModuleKey.Settings);

	/// <inheritdoc />
	protected override void OnValidationError(ShodanRequestValidationException ex) => ErrorMessage = ex.Message;
}
