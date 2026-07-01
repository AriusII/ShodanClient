using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShodanClient.App.Services.AccountSession;
using ShodanClient.App.Services.Notifications;
using ShodanClient.App.Services.ShodanClientAccessor;
using ShodanClient.Domain.Dns;

namespace ShodanClient.App.ViewModels.Dns;

/// <summary>
///     DNS lookup/resolve/reverse tools: three independent tabs sharing this one view model — a
///     domain lookup, a hostname-&gt;IP resolver, and an IP-&gt;hostname reverse resolver.
/// </summary>
public sealed partial class DnsViewModel : ModuleViewModelBase
{
	private readonly IAccountSessionService _accountSession;
	private readonly INotificationService _notifications;

	/// <summary>Creates the DNS Tools module view model.</summary>
	public DnsViewModel(INotificationService notifications, IShodanClientAccessor accessor,
		IAccountSessionService accountSession)
		: base(notifications)
	{
		_notifications = notifications;
		_accountSession = accountSession;
		Accessor = accessor;
		Title = "DNS Tools";
	}

	[ObservableProperty] public partial bool DomainHasMore { get; set; }

	[ObservableProperty] public partial string DomainInput { get; set; } = string.Empty;

	[ObservableProperty] public partial int DomainPage { get; set; } = 1;

	[ObservableProperty] public partial bool IncludeHistory { get; set; }

	[ObservableProperty] public partial string ResolveHostnamesInput { get; set; } = string.Empty;

	[ObservableProperty] public partial string ReverseIpsInput { get; set; } = string.Empty;

	[ObservableProperty] public partial string SelectedRecordType { get; set; } = "All";

	/// <summary>The active Shodan client accessor.</summary>
	public IShodanClientAccessor Accessor { get; }

	/// <summary>Record type filter choices for the Domain lookup tab; <c>"All"</c> means no filter.</summary>
	public IReadOnlyList<string> RecordTypeOptions { get; } = ["All", "A", "AAAA", "CNAME", "NS", "SOA", "MX", "TXT"];

	/// <summary>Classification tags returned for the last looked-up domain.</summary>
	public ObservableCollection<string> DomainTags { get; } = [];

	/// <summary>Known subdomains returned for the last looked-up domain.</summary>
	public ObservableCollection<string> DomainSubdomains { get; } = [];

	/// <summary>Individual DNS records returned for the last looked-up domain.</summary>
	public ObservableCollection<DnsRecord> DomainRecords { get; } = [];

	/// <summary>Hostname -&gt; IP results from the last Resolve run.</summary>
	public ObservableCollection<HostnameResolution> ResolveResults { get; } = [];

	/// <summary>IP -&gt; hostnames results from the last Reverse run.</summary>
	public ObservableCollection<IpReverseResolution> ReverseResults { get; } = [];

	/// <summary>Whether <see cref="PreviousDomainPageCommand" /> would do anything (there is a previous page).</summary>
	public bool CanGoToPreviousDomainPage => DomainPage > 1;

	/// <summary>Whether <see cref="NextDomainPageCommand" /> would do anything (<see cref="DomainHasMore" />).</summary>
	public bool CanGoToNextDomainPage => DomainHasMore;

	partial void OnDomainPageChanged(int value) => OnPropertyChanged(nameof(CanGoToPreviousDomainPage));

	partial void OnDomainHasMoreChanged(bool value) => OnPropertyChanged(nameof(CanGoToNextDomainPage));

	/// <summary>
	///     Looks up <see cref="DomainInput" /> (<c>GetDomainAsync</c>), resetting to page 1. Warns
	///     that this consumes 1 query credit and only runs once confirmed, mirroring
	///     <c>SearchModuleViewModel.LoadNextPage</c>'s confirm-before-spend pattern.
	/// </summary>
	[RelayCommand]
	private void LookupDomain()
	{
		var domain = DomainInput.Trim();
		if (domain.Length == 0)
		{
			_notifications.Warning("Enter a domain to look up.");
			return;
		}

		DomainInput = domain;
		_notifications.Warning(
			"Looking up a domain consumes 1 Shodan query credit.",
			"Confirm domain lookup",
			() =>
			{
				DomainPage = 1;
				_ = RunAsync(LoadDomainAsync);
			},
			"Look up");
	}

	/// <summary>Loads the previous page of DNS records for the current domain, if any.</summary>
	[RelayCommand]
	private void PreviousDomainPage()
	{
		if (DomainPage <= 1)
		{
			return;
		}

		_notifications.Warning(
			"Loading this page consumes 1 Shodan query credit.",
			"Confirm domain lookup",
			() =>
			{
				DomainPage--;
				_ = RunAsync(LoadDomainAsync);
			},
			"Load page");
	}

	/// <summary>Loads the next page of DNS records for the current domain, if <see cref="DomainHasMore" />.</summary>
	[RelayCommand]
	private void NextDomainPage()
	{
		if (!DomainHasMore)
		{
			return;
		}

		_notifications.Warning(
			"Loading the next page consumes 1 Shodan query credit.",
			"Confirm domain lookup",
			() =>
			{
				DomainPage++;
				_ = RunAsync(LoadDomainAsync);
			},
			"Load next page");
	}

	/// <summary>Resolves the newline/comma separated hostnames in <see cref="ResolveHostnamesInput" />.</summary>
	[RelayCommand]
	private Task ResolveAsync(CancellationToken cancellationToken)
	{
		var hostnames = ParseEntries(ResolveHostnamesInput);
		if (hostnames.Count != 0)
		{
			return RunAsync(async ct =>
			{
				if (Accessor.Client is not { } client)
				{
					_notifications.Warning("Connect a Shodan API key first.");
					return;
				}

				var result = await client.Dns.ResolveAsync(hostnames, ct).ConfigureAwait(true);
				ResolveResults.Clear();
				foreach (var (hostname, ip) in result)
				{
					ResolveResults.Add(new HostnameResolution(hostname, ip));
				}
			}, cancellationToken);
		}

		_notifications.Warning("Enter at least one hostname to resolve.");
		return Task.CompletedTask;
	}

	/// <summary>Reverse-resolves the newline/comma separated IPs in <see cref="ReverseIpsInput" />.</summary>
	[RelayCommand]
	private Task ReverseAsync(CancellationToken cancellationToken)
	{
		var ips = ParseEntries(ReverseIpsInput);
		if (ips.Count != 0)
		{
			return RunAsync(async ct =>
			{
				if (Accessor.Client is not { } client)
				{
					_notifications.Warning("Connect a Shodan API key first.");
					return;
				}

				var result = await client.Dns.ReverseAsync(ips, ct).ConfigureAwait(true);
				ReverseResults.Clear();
				foreach (var (ip, hostnames) in result)
				{
					ReverseResults.Add(new IpReverseResolution(ip, hostnames));
				}
			}, cancellationToken);
		}

		_notifications.Warning("Enter at least one IP address to reverse-resolve.");
		return Task.CompletedTask;
	}

	private async Task LoadDomainAsync(CancellationToken cancellationToken)
	{
		if (Accessor.Client is not { } client)
		{
			_notifications.Warning("Connect a Shodan API key first.");
			return;
		}

		var domain = DomainInput.Trim();
		if (domain.Length == 0)
		{
			return;
		}

		var type = SelectedRecordType == "All" ? null : SelectedRecordType;
		var info = await client.Dns.GetDomainAsync(domain, IncludeHistory, type, DomainPage, cancellationToken)
			.ConfigureAwait(true);

		DomainTags.Clear();
		foreach (var tag in info.Tags)
		{
			DomainTags.Add(tag);
		}

		DomainSubdomains.Clear();
		foreach (var subdomain in info.Subdomains)
		{
			DomainSubdomains.Add(subdomain);
		}

		DomainRecords.Clear();
		foreach (var record in info.Data)
		{
			DomainRecords.Add(record);
		}

		DomainHasMore = info.More;

		await _accountSession.RefreshAsync(true, cancellationToken).ConfigureAwait(true);
	}

	private static List<string> ParseEntries(string input) =>
		input.Split([',', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToList();
}
