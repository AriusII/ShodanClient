using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShodanClient.App.Services.Notifications;
using ShodanClient.App.Services.ShodanClientAccessor;
using ShodanClient.Domain.ApiStatus;

namespace ShodanClient.App.ViewModels.Diagnostics;

/// <summary>
///     Troubleshooting surface: the caller's public IP, the HTTP headers Shodan sees, the raw
///     plan/credit detail from <c>ApiInfo</c>, and the app's own version.
/// </summary>
public sealed partial class DiagnosticsViewModel : ModuleViewModelBase
{
	/// <summary>Creates the Diagnostics module view model.</summary>
	public DiagnosticsViewModel(INotificationService notifications, IShodanClientAccessor accessor)
		: base(notifications)
	{
		Accessor = accessor;
		Title = "Diagnostics";
		AppVersion = typeof(DiagnosticsViewModel).Assembly.GetName().Version;

		_ = RefreshCommand.ExecuteAsync(null);
	}

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(QueryCreditsDisplay))]
	[NotifyPropertyChangedFor(nameof(ScanCreditsDisplay))]
	[NotifyPropertyChangedFor(nameof(HttpsTelnetDisplay))]
	[NotifyPropertyChangedFor(nameof(UnlockedDisplay))]
	[NotifyPropertyChangedFor(nameof(MonitoredIpsDisplay))]
	public partial ApiInfo? ApiInfo { get; set; }

	[ObservableProperty] public partial string? MyIp { get; set; }

	/// <summary>The active Shodan client accessor.</summary>
	public IShodanClientAccessor Accessor { get; }

	/// <summary>The app's own assembly version, for bug reports.</summary>
	public Version? AppVersion { get; }

	/// <summary>The HTTP headers Shodan observed for the last request, sorted by name.</summary>
	public ObservableCollection<HttpHeaderEntry> HttpHeaders { get; } = [];

	/// <summary>Remaining query credits against the plan's ceiling, e.g. <c>"100 (100)"</c>.</summary>
	public string QueryCreditsDisplay => ApiInfo is null
		? "-"
		: $"{ApiInfo.QueryCredits} ({ApiInfo.UsageLimits.QueryCredits})";

	/// <summary>Remaining scan credits against the plan's ceiling.</summary>
	public string ScanCreditsDisplay => ApiInfo is null
		? "-"
		: $"{ApiInfo.ScanCredits} ({ApiInfo.UsageLimits.ScanCredits})";

	/// <summary>Whether the plan can access HTTPS-only and Telnet-only results.</summary>
	public string HttpsTelnetDisplay => ApiInfo is null
		? "-"
		: $"{ToYesNo(ApiInfo.Https)} / {ToYesNo(ApiInfo.Telnet)}";

	/// <summary>Whether the account is unlocked, and how many unlocks remain.</summary>
	public string UnlockedDisplay => ApiInfo is null
		? "-"
		: $"{ToYesNo(ApiInfo.Unlocked)} ({ApiInfo.UnlockedLeft} left)";

	/// <summary>The plan's monitored-IP ceiling, if applicable.</summary>
	public string MonitoredIpsDisplay => ApiInfo?.MonitoredIps?.ToString(CultureInfo.InvariantCulture) ?? "N/A";

	/// <summary>Reloads My-IP, HTTP headers and the raw usage-limit detail.</summary>
	[RelayCommand]
	private Task RefreshAsync(CancellationToken cancellationToken) => RunAsync(async ct =>
	{
		if (Accessor.Client is not { } client)
		{
			return;
		}

		var myIpTask = client.Tools.GetMyIpAsync(ct);
		var headersTask = client.Tools.GetHttpHeadersAsync(ct);
		var apiInfoTask = client.ApiInfo.GetAsync(ct);
		await Task.WhenAll(myIpTask, headersTask, apiInfoTask).ConfigureAwait(true);

		MyIp = myIpTask.Result;
		ApiInfo = apiInfoTask.Result;

		HttpHeaders.Clear();
		foreach (var header in headersTask.Result.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
		{
			HttpHeaders.Add(new HttpHeaderEntry(header.Key, header.Value));
		}
	}, cancellationToken);

	private static string ToYesNo(bool value) => value ? "Yes" : "No";
}
