using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShodanClient.App.Services.AccountSession;
using ShodanClient.App.Services.Notifications;
using ShodanClient.App.Services.ShodanClientAccessor;
using ShodanClient.Application.Exceptions;
using ShodanClient.Domain.Scanning;

namespace ShodanClient.App.ViewModels.Scans;

/// <summary>
///     On-demand scans and crawl status: a reference panel of crawled ports/protocols, a "request
///     scan" form for specific IPs/netblocks, an Enterprise-gated Internet-wide scan, and a polled
///     list of active scans.
/// </summary>
public sealed partial class ScansViewModel : ModuleViewModelBase
{
	private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(7);

	private readonly IAccountSessionService _accountSession;
	private readonly INotificationService _notifications;

	private CancellationTokenSource? _pollingCts;

	/// <summary>Creates the Scans module view model.</summary>
	public ScansViewModel(INotificationService notifications, IShodanClientAccessor accessor,
		IAccountSessionService accountSession)
		: base(notifications)
	{
		_notifications = notifications;
		_accountSession = accountSession;
		Accessor = accessor;
		Title = "Scans";

		_ = RunAsync(async ct =>
		{
			await LoadReferenceDataCoreAsync(ct).ConfigureAwait(true);
			await RefreshActiveScansCoreAsync(ct).ConfigureAwait(true);
		});
	}

	[ObservableProperty] public partial string InternetScanPortInput { get; set; } = string.Empty;

	[ObservableProperty] public partial bool IsEnterprise { get; set; } = true;

	[ObservableProperty] public partial bool IsInternetScanConfirmationPending { get; set; }

	[ObservableProperty] public partial bool IsScanConfirmationPending { get; set; }

	[ObservableProperty] public partial string ScanTargetsInput { get; set; } = string.Empty;

	[ObservableProperty] public partial ProtocolInfo? SelectedInternetScanProtocol { get; set; }

	/// <summary>The active Shodan client accessor.</summary>
	public IShodanClientAccessor Accessor { get; }

	/// <summary>Ports Shodan crawls, loaded once (<c>GetCrawledPortsAsync</c>).</summary>
	public ObservableCollection<int> CrawledPorts { get; } = [];

	/// <summary>Protocols available for scanning, loaded once (<c>GetProtocolsAsync</c>).</summary>
	public ObservableCollection<ProtocolInfo> Protocols { get; } = [];

	/// <summary>Currently active on-demand scans (<c>ListScansAsync</c>), refreshed by polling.</summary>
	public ObservableCollection<ScanListEntry> ActiveScans { get; } = [];

	partial void OnScanTargetsInputChanged(string value) => IsScanConfirmationPending = false;

	partial void OnInternetScanPortInputChanged(string value) => IsInternetScanConfirmationPending = false;

	partial void OnSelectedInternetScanProtocolChanged(ProtocolInfo? value) =>
		IsInternetScanConfirmationPending = false;

	/// <summary>
	///     Requests an on-demand crawl of <see cref="ScanTargetsInput" />. The first click only arms a
	///     confirmation (a warning toast naming the estimated credit cost); the second click submits the
	///     scan. <see cref="ScanSubmission.Count" />/<see cref="ScanSubmission.CreditsLeft" />, shown after
	///     submission, are the only authoritative numbers — netblocks (CIDR) expand to many IPs server-side
	///     and Shodan may skip IPs it crawled in the last 24 hours, so the pre-submission figure below is
	///     an estimate only.
	/// </summary>
	[RelayCommand]
	private Task RequestScanAsync(CancellationToken cancellationToken)
	{
		var targets = ParseTargets(ScanTargetsInput);
		if (targets.Count == 0)
		{
			_notifications.Warning("Enter at least one IP address or CIDR block to scan.");
			return Task.CompletedTask;
		}

		if (!IsScanConfirmationPending)
		{
			IsScanConfirmationPending = true;
			var estimatedIps = EstimateTotalIpCount(targets);
			var costDescription = estimatedIps == targets.Count
				? $"consuming up to {targets.Count} scan credit(s)"
				: $"expanding to an estimated {estimatedIps:N0} IP address(es) and consuming up to {estimatedIps:N0} scan credit(s) (1 per IP)";
			_notifications.Warning(
				$"This will scan {targets.Count} target(s), {costDescription}. Shodan may skip IPs crawled in the last 24 hours, so actual usage may be lower. Click \"Request Scan\" again to confirm.",
				"Confirm scan request");
			return Task.CompletedTask;
		}

		IsScanConfirmationPending = false;
		return RunAsync(async ct =>
		{
			if (Accessor.Client is not { } client)
			{
				_notifications.Warning("Connect a Shodan API key first.");
				return;
			}

			var submission = await client.Scans.RequestScanAsync(targets, ct).ConfigureAwait(true);
			ScanTargetsInput = string.Empty;
			_notifications.Success(
				$"Scan {submission.Id} submitted for {submission.Count} target(s). {submission.CreditsLeft} scan credit(s) left.");

			await _accountSession.RefreshAsync(true, ct).ConfigureAwait(true);
			await RefreshActiveScansCoreAsync(ct).ConfigureAwait(true);
		}, cancellationToken);
	}

	/// <summary>
	///     Requests an Internet-wide crawl of <see cref="InternetScanPortInput" />/<see cref="SelectedInternetScanProtocol" />
	///     . Like <see cref="RequestScanAsync" />, the first click only arms a confirmation naming the
	///     credit cost; the second click submits the crawl. Requires an Enterprise/researcher plan; a 403
	///     flips <see cref="IsEnterprise" /> to hide the section.
	/// </summary>
	[RelayCommand]
	private Task ScanInternetAsync(CancellationToken cancellationToken)
	{
		if (!int.TryParse(InternetScanPortInput, out var port) || port is < 1 or > 65535)
		{
			_notifications.Warning("Enter a valid port number (1-65535).");
			return Task.CompletedTask;
		}

		if (SelectedInternetScanProtocol is not { } protocol)
		{
			_notifications.Warning("Select a protocol to scan with.");
			return Task.CompletedTask;
		}

		if (!IsInternetScanConfirmationPending)
		{
			IsInternetScanConfirmationPending = true;
			_notifications.Warning(
				$"This requests an Internet-wide crawl on port {port}/{protocol.Name}, consuming 1 scan credit for every matching IP Shodan finds — this can use a very large number of scan credits. Click \"Scan Internet\" again to confirm.",
				"Confirm Internet-wide scan");
			return Task.CompletedTask;
		}

		IsInternetScanConfirmationPending = false;
		return RunAsync(async ct =>
		{
			if (Accessor.Client is not { } client)
			{
				_notifications.Warning("Connect a Shodan API key first.");
				return;
			}

			try
			{
				var result = await client.Scans.ScanInternetAsync(port, protocol.Name, ct).ConfigureAwait(true);
				InternetScanPortInput = string.Empty;
				SelectedInternetScanProtocol = null;
				_notifications.Success($"Internet scan {result.Id} submitted for port {port}/{protocol.Name}.");
				await _accountSession.RefreshAsync(true, ct).ConfigureAwait(true);
			}
			catch (ShodanAccessDeniedException)
			{
				IsEnterprise = false;
				_notifications.Warning("Internet-wide scanning requires an Enterprise/researcher plan.",
					"Enterprise feature");
			}
		}, cancellationToken);
	}

	/// <summary>Reloads the active scans list on demand.</summary>
	[RelayCommand]
	private Task RefreshActiveScansAsync(CancellationToken cancellationToken) =>
		RunAsync(RefreshActiveScansCoreAsync, cancellationToken);

	private async Task LoadReferenceDataCoreAsync(CancellationToken cancellationToken)
	{
		if (Accessor.Client is not { } client)
		{
			return;
		}

		var portsTask = client.Scans.GetCrawledPortsAsync(cancellationToken);
		var protocolsTask = client.Scans.GetProtocolsAsync(cancellationToken);
		await Task.WhenAll(portsTask, protocolsTask).ConfigureAwait(true);

		CrawledPorts.Clear();
		foreach (var port in portsTask.Result.Order())
		{
			CrawledPorts.Add(port);
		}

		Protocols.Clear();
		foreach (var (name, description) in protocolsTask.Result.OrderBy(entry => entry.Key,
					 StringComparer.OrdinalIgnoreCase))
		{
			Protocols.Add(new ProtocolInfo(name, description));
		}
	}

	private async Task RefreshActiveScansCoreAsync(CancellationToken cancellationToken)
	{
		if (Accessor.Client is not { } client)
		{
			return;
		}

		var scans = await client.Scans.ListScansAsync(cancellationToken).ConfigureAwait(true);
		ActiveScans.Clear();
		foreach (var scan in scans)
		{
			ActiveScans.Add(scan);
		}

		UpdatePolling();
	}

	private void UpdatePolling()
	{
		if (ActiveScans.Any(scan => !IsTerminalStatus(scan.Status)))
		{
			StartPolling();
		}
		else
		{
			StopPolling();
		}
	}

	private void StartPolling()
	{
		if (_pollingCts is not null)
		{
			return;
		}

		var cts = new CancellationTokenSource();
		_pollingCts = cts;
		_ = RunPollingLoopAsync(cts.Token);
	}

	private void StopPolling()
	{
		var cts = _pollingCts;
		_pollingCts = null;
		cts?.Cancel();
		cts?.Dispose();
	}

	private async Task RunPollingLoopAsync(CancellationToken cancellationToken)
	{
		try
		{
			using var timer = new PeriodicTimer(PollingInterval);
			while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(true))
			{
				await PollActiveScanStatusesAsync(cancellationToken).ConfigureAwait(true);
				if (!ActiveScans.All(scan => IsTerminalStatus(scan.Status)))
				{
					continue;
				}

				StopPolling();
				return;
			}
		}
		catch (OperationCanceledException)
		{
			// Expected when polling is stopped because every active scan reached a terminal status.
		}
	}

	private async Task PollActiveScanStatusesAsync(CancellationToken cancellationToken)
	{
		if (Accessor.Client is not { } client)
		{
			StopPolling();
			return;
		}

		for (var i = 0; i < ActiveScans.Count; i++)
		{
			var entry = ActiveScans[i];
			if (IsTerminalStatus(entry.Status))
			{
				continue;
			}

			try
			{
				var status = await client.Scans.GetScanStatusAsync(entry.Id, cancellationToken).ConfigureAwait(true);
				if (!string.Equals(status.Status, entry.Status, StringComparison.Ordinal))
				{
					ActiveScans[i] = entry with { Status = status.Status };
				}
			}
			catch (ShodanException)
			{
				// Best-effort background poll: leave the last known status in place and try again
				// on the next tick.
			}
		}
	}

	private static bool IsTerminalStatus(string status) =>
		string.Equals(status, "DONE", StringComparison.OrdinalIgnoreCase);

	private static List<string> ParseTargets(string input) =>
		input.Split([',', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToList();

	/// <summary>
	///     Estimates the total number of IPs covered by <paramref name="targets" />, expanding any
	///     CIDR netblocks — each IP within a netblock consumes its own scan credit per
	///     <c>IScanService.RequestScanAsync</c>'s "each IP consumes one scan credit" contract. Single
	///     IPs (and anything that doesn't parse as a netblock) count as 1. This is only a pre-submission
	///     estimate; the authoritative post-submission cost is always <see cref="ScanSubmission.Count" />.
	/// </summary>
	private static long EstimateTotalIpCount(IReadOnlyList<string> targets)
	{
		long total = 0;
		foreach (var target in targets)
		{
			total += EstimateIpCount(target);
		}

		return total;
	}

	private static long EstimateIpCount(string target)
	{
		var slashIndex = target.IndexOf('/');
		if (slashIndex < 0)
		{
			return 1;
		}

		var addressPart = target[..slashIndex];
		var prefixPart = target[(slashIndex + 1)..];
		if (!IPAddress.TryParse(addressPart, out var address) || !int.TryParse(prefixPart, out var prefixLength))
		{
			return 1;
		}

		var bits = address.AddressFamily == AddressFamily.InterNetworkV6 ? 128 : 32;
		if (prefixLength < 0 || prefixLength > bits)
		{
			return 1;
		}

		var hostBits = bits - prefixLength;

		// Guard against overflow for enormous IPv6 netblocks; anything this large is already an
		// unambiguous "this is enormous" signal for the warning copy.
		return hostBits >= 62 ? long.MaxValue / 2 : 1L << hostBits;
	}
}
