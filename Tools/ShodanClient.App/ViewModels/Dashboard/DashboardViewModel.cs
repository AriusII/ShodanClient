using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShodanClient.App.Services.AccountSession;
using ShodanClient.App.Services.Navigation;
using ShodanClient.App.Services.Notifications;
using ShodanClient.App.Services.ShodanClientAccessor;
using ShodanClient.Application.Exceptions;
using ShodanClient.Domain.InternetDb;

namespace ShodanClient.App.ViewModels.Dashboard;

/// <summary>
///     Landing page: the current <see cref="Session.AccountSnapshot" /> as a summary card, a quick
///     free-tier InternetDB lookup, and shortcuts into the Search and Host Lookup modules.
/// </summary>
public sealed partial class DashboardViewModel : ModuleViewModelBase
{
	/// <summary>Creates the Dashboard module view model.</summary>
	public DashboardViewModel(
		INotificationService notifications,
		IShodanClientAccessor accessor,
		IAccountSessionService accountSession,
		INavigationService navigation)
		: base(notifications)
	{
		Accessor = accessor;
		AccountSession = accountSession;
		Navigation = navigation;
		Title = "Dashboard";
	}

	[ObservableProperty] public partial string IpInput { get; set; } = string.Empty;

	[ObservableProperty] public partial string? LookupError { get; set; }

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(PortsDisplay))]
	public partial InternetDbHost? LookupResult { get; set; }

	/// <summary>The active Shodan client accessor.</summary>
	public IShodanClientAccessor Accessor { get; }

	/// <summary>The account/plan tracking service.</summary>
	public IAccountSessionService AccountSession { get; }

	/// <summary>The shell's navigation service.</summary>
	public INavigationService Navigation { get; }

	/// <summary>A comma-separated view of <see cref="LookupResult" />'s open ports, for a simple summary line.</summary>
	public string PortsDisplay => LookupResult is null || LookupResult.Ports.Count == 0
		? "None"
		: string.Join(", ", LookupResult.Ports);

	/// <summary>Refreshes the account/plan snapshot shown in the summary card.</summary>
	[RelayCommand]
	private Task RefreshAccountAsync(CancellationToken cancellationToken) =>
		RunAsync(ct => AccountSession.RefreshAsync(true, ct), cancellationToken);

	/// <summary>
	///     Looks up <see cref="IpInput" /> against the free InternetDB dataset. Uses
	///     <see cref="IShodanClientAccessor.GetAnonymousClientAsync" /> rather than the (possibly
	///     unconfigured) <see cref="Accessor" />.Client, so this genuinely never requires an API key.
	/// </summary>
	[RelayCommand]
	private Task LookupInternetDbAsync(CancellationToken cancellationToken)
	{
		LookupError = null;
		var ip = IpInput.Trim();
		if (ip.Length != 0)
		{
			return RunAsync(async ct =>
			{
				var client = await Accessor.GetAnonymousClientAsync(ct).ConfigureAwait(true);
				LookupResult = await client.InternetDb.GetAsync(ip, ct).ConfigureAwait(true);
			}, cancellationToken);
		}

		LookupError = "Enter an IP address to look up.";
		return Task.CompletedTask;
	}

	/// <summary>Navigates to the Search module.</summary>
	[RelayCommand]
	private void GoToSearch() => Navigation.NavigateTo(ModuleKey.Search);

	/// <summary>Navigates to the Host Lookup module.</summary>
	[RelayCommand]
	private void GoToHostLookup() => Navigation.NavigateTo(ModuleKey.HostLookup);

	/// <inheritdoc />
	protected override void OnValidationError(ShodanRequestValidationException ex) => LookupError = ex.Message;
}
