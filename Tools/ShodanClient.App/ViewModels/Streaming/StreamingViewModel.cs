using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShodanClient.App.Services.Navigation;
using ShodanClient.App.Services.Notifications;
using ShodanClient.App.Services.ShodanClientAccessor;
using ShodanClient.Application.Exceptions;
using ShodanClient.Domain.Search;

namespace ShodanClient.App.ViewModels.Streaming;

/// <summary>
///     Live banner streaming across the 8 <c>IStreamService</c> feeds. Start/Stop drive an internal
///     <see cref="CancellationTokenSource" /> around a background consumption loop so the UI thread is
///     never blocked; <see cref="LiveMatches" /> is capped to bound memory during a long-running stream.
///     Also the deep-link target for Alerts' "Stream this alert" action via
///     <see cref="INavigationAware{TParam}" />.
/// </summary>
public sealed partial class StreamingViewModel : ModuleViewModelBase, INavigationAware<string?>
{
	private const int MaxLiveMatches = 2000;

	private readonly INavigationService _navigation;
	private readonly INotificationService _notifications;
	private readonly Queue<DateTimeOffset> _recentTimestamps = new();

	private CancellationTokenSource? _cts;

	/// <summary>Creates the Streaming module view model.</summary>
	public StreamingViewModel(INotificationService notifications, IShodanClientAccessor accessor,
		INavigationService navigation)
		: base(notifications)
	{
		_notifications = notifications;
		Accessor = accessor;
		_navigation = navigation;
		Accessor.Changed += OnAccessorChanged;
		Title = "Streaming";
	}

	[ObservableProperty] public partial string AlertIdInput { get; set; } = string.Empty;

	[ObservableProperty] public partial string AsnsInput { get; set; } = string.Empty;

	[ObservableProperty] public partial double BannersPerSecond { get; set; }

	[ObservableProperty] public partial string CountriesInput { get; set; } = string.Empty;

	[ObservableProperty] public partial bool IsStreaming { get; set; }

	[ObservableProperty] public partial StreamMode Mode { get; set; } = StreamMode.AllBanners;

	[ObservableProperty] public partial string PortsInput { get; set; } = string.Empty;

	[ObservableProperty] public partial string QueryInput { get; set; } = string.Empty;

	[ObservableProperty] public partial Banner? SelectedBanner { get; set; }

	[ObservableProperty] public partial string VulnerabilitiesInput { get; set; } = string.Empty;

	/// <summary>The active Shodan client accessor.</summary>
	public IShodanClientAccessor Accessor { get; }

	/// <summary>Every selectable stream mode, bound to the mode picker.</summary>
	public StreamMode[] AvailableModes { get; } = Enum.GetValues<StreamMode>();

	/// <summary>
	///     Banners collected so far in the current (or most recent) stream, oldest first, capped at
	///     <see cref="MaxLiveMatches" />.
	/// </summary>
	public ObservableCollection<Banner> LiveMatches { get; } = [];

	/// <summary>Whether <see cref="Mode" /> needs a set of ASNs.</summary>
	public bool IsAsnMode => Mode == StreamMode.Asn;

	/// <summary>Whether <see cref="Mode" /> needs a set of country codes.</summary>
	public bool IsCountriesMode => Mode == StreamMode.Countries;

	/// <summary>Whether <see cref="Mode" /> needs a set of ports.</summary>
	public bool IsPortsMode => Mode == StreamMode.Ports;

	/// <summary>Whether <see cref="Mode" /> needs a set of CVE identifiers.</summary>
	public bool IsVulnerabilitiesMode => Mode == StreamMode.Vulnerabilities;

	/// <summary>Whether <see cref="Mode" /> needs a raw Shodan query.</summary>
	public bool IsQueryMode => Mode == StreamMode.Query;

	/// <summary>Whether <see cref="Mode" /> needs a single alert identifier.</summary>
	public bool IsAlertMode => Mode == StreamMode.Alert;

	/// <inheritdoc />
	/// <remarks>
	///     Deep-link entry point for Alerts' "Stream this alert": preselects <see cref="StreamMode.Alert" />,
	///     prefills <see cref="AlertIdInput" /> and starts the stream immediately if one isn't already
	///     running. NavigationService invokes this on every navigation to this module, including plain
	///     nav-item clicks that pass no parameter, in which case the cast in the default interface
	///     implementation hands back <see langword="null" /> and this is a no-op.
	/// </remarks>
	public void OnNavigatedTo(string? parameter)
	{
		if (string.IsNullOrWhiteSpace(parameter))
		{
			return;
		}

		Mode = StreamMode.Alert;
		AlertIdInput = parameter;
		if (StartCommand.CanExecute(null))
		{
			StartCommand.Execute(null);
		}
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

	partial void OnModeChanged(StreamMode value)
	{
		OnPropertyChanged(nameof(IsAsnMode));
		OnPropertyChanged(nameof(IsCountriesMode));
		OnPropertyChanged(nameof(IsPortsMode));
		OnPropertyChanged(nameof(IsVulnerabilitiesMode));
		OnPropertyChanged(nameof(IsQueryMode));
		OnPropertyChanged(nameof(IsAlertMode));
	}

	partial void OnIsStreamingChanged(bool value)
	{
		StartCommand.NotifyCanExecuteChanged();
		StopCommand.NotifyCanExecuteChanged();
	}

	/// <summary>Starts consuming the selected mode's feed on a background task.</summary>
	[RelayCommand(CanExecute = nameof(CanStart))]
	private void Start()
	{
		var cts = new CancellationTokenSource();
		_cts = cts;
		IsStreaming = true;
		_ = RunStreamAsync(cts.Token);
	}

	private bool CanStart() => !IsStreaming && Accessor.IsConfigured;

	/// <summary>Cancels the active stream, if any.</summary>
	[RelayCommand(CanExecute = nameof(CanStop))]
	private void Stop()
	{
		var cts = _cts;
		_cts = null;
		IsStreaming = false;
		cts?.Cancel();
		cts?.Dispose();
	}

	private bool CanStop() => IsStreaming;

	private async Task RunStreamAsync(CancellationToken cancellationToken)
	{
		try
		{
			await RunAsync(async ct =>
			{
				if (Accessor.Client is not { } client)
				{
					_notifications.Warning("Connect a Shodan API key first (see Settings).");
					return;
				}

				var stream = BuildStream(client);
				await foreach (var banner in stream.WithCancellation(ct).ConfigureAwait(true))
				{
					AppendBanner(banner);
				}
			}, cancellationToken).ConfigureAwait(true);
		}
		catch (OperationCanceledException)
		{
			// Expected: the user pressed Stop.
		}
		finally
		{
			// If the stream ended on its own (completed, or an error already reported by RunAsync)
			// rather than via Stop(), the flag and CancellationTokenSource still need cleaning up.
			if (IsStreaming)
			{
				Stop();
			}
		}
	}

	private IAsyncEnumerable<Banner> BuildStream(IShodanClient client) => Mode switch
	{
		StreamMode.AllBanners => client.Stream.StreamAllBannersAsync(),
		StreamMode.Asn => client.Stream.StreamByAsnAsync(SplitInput(AsnsInput)),
		StreamMode.Countries => client.Stream.StreamByCountriesAsync(SplitInput(CountriesInput)),
		StreamMode.Ports => client.Stream.StreamByPortsAsync(ParsePorts(PortsInput)),
		StreamMode.Vulnerabilities => client.Stream.StreamByVulnerabilitiesAsync(SplitInput(VulnerabilitiesInput)),
		StreamMode.Query => client.Stream.StreamByQueryAsync(QueryInput.Trim()),
		StreamMode.Alerts => client.Stream.StreamAlertsAsync(),
		StreamMode.Alert => client.Stream.StreamAlertAsync(AlertIdInput.Trim()),
		_ => throw new InvalidOperationException($"Unknown stream mode: {Mode}.")
	};

	private static string[] SplitInput(string input) =>
		input.Split([',', ' ', '\t', '\r', '\n'],
			StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

	private static List<int> ParsePorts(string input)
	{
		var parts = SplitInput(input);
		var ports = new List<int>(parts.Length);
		foreach (var part in parts)
		{
			if (!int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var port))
			{
				throw new ShodanRequestValidationException($"'{part}' is not a valid port number.");
			}

			if (port is < 0 or > 65535)
			{
				throw new ShodanRequestValidationException(
					$"'{part}' is not a valid port number (must be between 0 and 65535).");
			}

			ports.Add(port);
		}

		return ports;
	}

	private void AppendBanner(Banner banner)
	{
		if (Dispatcher.UIThread.CheckAccess())
		{
			AppendBannerCore(banner);
		}
		else
		{
			Dispatcher.UIThread.Post(() => AppendBannerCore(banner));
		}
	}

	private void AppendBannerCore(Banner banner)
	{
		LiveMatches.Add(banner);
		while (LiveMatches.Count > MaxLiveMatches)
		{
			LiveMatches.RemoveAt(0);
		}

		var now = DateTimeOffset.UtcNow;
		_recentTimestamps.Enqueue(now);
		var cutoff = now - TimeSpan.FromSeconds(1);
		while (_recentTimestamps.Count > 0 && _recentTimestamps.Peek() < cutoff)
		{
			_recentTimestamps.Dequeue();
		}

		BannersPerSecond = _recentTimestamps.Count;
	}

	private void OnAccessorChanged() => StartCommand.NotifyCanExecuteChanged();
}
