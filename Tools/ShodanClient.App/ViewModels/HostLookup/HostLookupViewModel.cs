using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShodanClient.App.Services.Navigation;
using ShodanClient.App.Services.Notifications;
using ShodanClient.App.Services.ShodanClientAccessor;
using ShodanClient.Domain.InternetDb;
using ShodanClient.Domain.Search;

namespace ShodanClient.App.ViewModels.HostLookup;

/// <summary>
///     Manual host lookup: the full-detail <c>Hosts.GetAsync</c> (requires an API key; per Shodan's
///     REST API this single-host endpoint does not itself spend query credits, unlike paginated
///     search) versus the free, key-less InternetDB summary. Also the natural drill-down target when a
///     row is opened elsewhere (Search, Streaming, …) via <see cref="INavigationAware{TParam}" />.
/// </summary>
public sealed partial class HostLookupViewModel : ModuleViewModelBase, INavigationAware<string?>
{
	private readonly INavigationService _navigation;
	private readonly INotificationService _notifications;

	/// <summary>Creates the Host Lookup module view model.</summary>
	public HostLookupViewModel(INotificationService notifications, IShodanClientAccessor accessor,
		INavigationService navigation)
		: base(notifications)
	{
		_notifications = notifications;
		Accessor = accessor;
		_navigation = navigation;
		Title = "Host Lookup";
	}

	[ObservableProperty] public partial InternetDbHost? DbResult { get; set; }

	[ObservableProperty] public partial bool IncludeHistory { get; set; }

	[ObservableProperty] public partial string Ip { get; set; } = string.Empty;

	[ObservableProperty] public partial Host? Result { get; set; }

	[ObservableProperty] public partial bool UseInternetDb { get; set; }

	/// <summary>The active Shodan client accessor.</summary>
	public IShodanClientAccessor Accessor { get; }

	/// <summary>Whether either lookup mode currently has a result to show (gates the empty-state placeholder).</summary>
	public bool HasResult => Result is not null || DbResult is not null;

	/// <inheritdoc />
	public void OnNavigatedTo(string? parameter)
	{
		// NavigationService invokes this on every navigation to this module, including plain nav-item
		// clicks and shortcuts that pass no parameter (e.g. Dashboard's "Go to Host Lookup"), in which
		// case the cast in the default interface implementation hands back null: only prefill/auto-run
		// when a real IP was supplied.
		if (string.IsNullOrWhiteSpace(parameter))
		{
			return;
		}

		Ip = parameter;
		LookupCommand.Execute(null);
	}

	partial void OnResultChanged(Host? value) => OnPropertyChanged(nameof(HasResult));

	partial void OnDbResultChanged(InternetDbHost? value) => OnPropertyChanged(nameof(HasResult));

	/// <summary>
	///     Clears whichever result belongs to the mode being switched away from, so flipping the toggle
	///     doesn't leave a stale panel from the previous mode visible until the next lookup completes.
	/// </summary>
	partial void OnUseInternetDbChanged(bool value)
	{
		Result = null;
		DbResult = null;
	}

	/// <summary>Looks up <see cref="Ip" />, either against the free InternetDB dataset or the full Shodan host API.</summary>
	[RelayCommand]
	private Task LookupAsync(CancellationToken cancellationToken)
	{
		var ip = Ip.Trim();
		if (ip.Length != 0)
		{
			return RunAsync(async ct =>
			{
				// InternetDB is a free, key-less dataset (see IShodanClientAccessor.GetAnonymousClientAsync),
				// so it must not be gated behind Accessor.Client — doing so would force "Connect a Shodan
				// API key first" onto a lookup that the UI (correctly) advertises as needing no key at all.
				if (UseInternetDb)
				{
					var anonymousClient = await Accessor.GetAnonymousClientAsync(ct).ConfigureAwait(true);
					DbResult = await anonymousClient.InternetDb.GetAsync(ip, ct).ConfigureAwait(true);
					Result = null;
					return;
				}

				if (Accessor.Client is not { } client)
				{
					_notifications.Warning("Connect a Shodan API key first (see Settings).");
					return;
				}

				Result = await client.Hosts.GetAsync(ip, IncludeHistory, cancellationToken: ct).ConfigureAwait(true);
				DbResult = null;
			}, cancellationToken);
		}

		_notifications.Warning("Enter an IP address to look up.");
		return Task.CompletedTask;
	}

	/// <summary>Opens the Exploits module pre-filled with a search for <paramref name="cveId" />.</summary>
	[RelayCommand]
	private void OpenInExploits(string? cveId)
	{
		if (!string.IsNullOrWhiteSpace(cveId))
		{
			_navigation.NavigateTo(ModuleKey.Exploits, $"cve:{cveId}");
		}
	}
}
