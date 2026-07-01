using CommunityToolkit.Mvvm.ComponentModel;
using ShodanClient.App.Services.ShodanClientAccessor;
using ShodanClient.App.Session;
using ShodanClient.Application.Exceptions;

namespace ShodanClient.App.Services.AccountSession;

/// <summary>
///     Default <see cref="IAccountSessionService" />. Refreshes on attach, on a 5-minute
///     <see cref="PeriodicTimer" /> while a client is attached, and on-demand with a 10-second
///     coalescing window (single-flight: concurrent callers share one in-flight request).
/// </summary>
public sealed partial class AccountSessionService : ObservableObject, IAccountSessionService, IAsyncDisposable
{
	private static readonly TimeSpan CoalesceWindow = TimeSpan.FromSeconds(10);
	private static readonly TimeSpan PeriodicInterval = TimeSpan.FromMinutes(5);

	private readonly IShodanClientAccessor _accessor;
	private readonly Lock _gate = new();

	private Task? _inFlightRefresh;

	private CancellationTokenSource? _periodicCts;

	/// <summary>Creates the account session service.</summary>
	public AccountSessionService(IShodanClientAccessor accessor)
	{
		_accessor = accessor;
		_accessor.Changed += OnAccessorChanged;

		if (_accessor.IsConfigured)
		{
			OnAccessorChanged();
		}
	}

	[ObservableProperty] public partial AccountSnapshot? Current { get; private set; }

	[ObservableProperty] public partial bool IsRefreshing { get; private set; }

	[ObservableProperty] public partial DateTimeOffset? LastRefreshedAt { get; private set; }

	/// <inheritdoc />
	public async Task RefreshAsync(bool force = false, CancellationToken cancellationToken = default)
	{
		if (_accessor.Client is not { } client)
		{
			Current = null;
			return;
		}

		Task refreshTask;
		lock (_gate)
		{
			if (!force && _inFlightRefresh is null && LastRefreshedAt is { } last &&
			    DateTimeOffset.UtcNow - last < CoalesceWindow)
			{
				return;
			}

			_inFlightRefresh ??= RunRefreshAsync(client, cancellationToken);
			refreshTask = _inFlightRefresh;
		}

		try
		{
			await refreshTask.ConfigureAwait(true);
		}
		finally
		{
			lock (_gate)
			{
				if (ReferenceEquals(_inFlightRefresh, refreshTask))
				{
					_inFlightRefresh = null;
				}
			}
		}
	}

	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		_accessor.Changed -= OnAccessorChanged;
		StopPeriodicRefresh();
		await Task.CompletedTask.ConfigureAwait(true);
	}

	private void OnAccessorChanged()
	{
		if (_accessor.IsConfigured)
		{
			StartPeriodicRefresh();
			_ = RefreshAsync(true);
		}
		else
		{
			StopPeriodicRefresh();
			Current = null;
		}
	}

	private async Task RunRefreshAsync(IShodanClient client, CancellationToken cancellationToken)
	{
		IsRefreshing = true;
		try
		{
			var profileTask = client.Account.GetProfileAsync(cancellationToken);
			var apiInfoTask = client.ApiInfo.GetAsync(cancellationToken);
			await Task.WhenAll(profileTask, apiInfoTask).ConfigureAwait(true);

			var profile = profileTask.Result;
			var apiInfo = apiInfoTask.Result;

			Current = new AccountSnapshot
			{
				Member = profile.Member,
				Credits = profile.Credits,
				DisplayName = profile.DisplayName,
				Plan = apiInfo.Plan,
				ScanCredits = apiInfo.ScanCredits,
				QueryCredits = apiInfo.QueryCredits,
				Https = apiInfo.Https,
				Telnet = apiInfo.Telnet,
				UsageLimits = apiInfo.UsageLimits
			};
			LastRefreshedAt = DateTimeOffset.UtcNow;
		}
		catch (ShodanException)
		{
			// Best-effort background refresh: leave the last known snapshot in place. The UI's
			// manual refresh action surfaces further attempts.
		}
		finally
		{
			IsRefreshing = false;
		}
	}

	private void StartPeriodicRefresh()
	{
		StopPeriodicRefresh();
		var cts = new CancellationTokenSource();
		_periodicCts = cts;
		_ = RunPeriodicLoopAsync(cts.Token);
	}

	private void StopPeriodicRefresh()
	{
		_periodicCts?.Cancel();
		_periodicCts?.Dispose();
		_periodicCts = null;
	}

	private async Task RunPeriodicLoopAsync(CancellationToken cancellationToken)
	{
		try
		{
			using var timer = new PeriodicTimer(PeriodicInterval);
			while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(true))
			{
				await RefreshAsync(true, cancellationToken).ConfigureAwait(true);
			}
		}
		catch (OperationCanceledException)
		{
			// Expected on detach/dispose.
		}
	}
}
