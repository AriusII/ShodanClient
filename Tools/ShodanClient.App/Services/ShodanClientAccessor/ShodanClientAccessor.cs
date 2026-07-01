using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShodanClient.App.Services.Settings;
using ShodanClient.Application.Configuration;
using ShodanClient.Application.Exceptions;
using ShodanClient.DependencyInjection;

namespace ShodanClient.App.Services.ShodanClientAccessor;

/// <summary>
///     Default <see cref="IShodanClientAccessor" />. Builds a small inner <see cref="IHost" /> per
///     attached API key (mirroring the SDK's own <c>AddShodanClient</c> registration pattern) so the
///     main app container never has to know whether a key is configured.
/// </summary>
public sealed class ShodanClientAccessor : IShodanClientAccessor, IAsyncDisposable
{
	// InternetDB's HttpClient deliberately never attaches the API-key handler (see
	// ShodanHttpClientRegistration.AddInternetDbClient in the SDK), so this placeholder is only ever
	// used to satisfy ShodanClientOptions.ApiKey's non-empty validation — it is never transmitted.
	private const string AnonymousPlaceholderApiKey = "anonymous-internetdb-only";

	private readonly SemaphoreSlim _anonymousGate = new(1, 1);
	private IShodanClient? _anonymousClient;
	private IHost? _anonymousHost;
	private IHost? _innerHost;

	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		if (_innerHost is { } host)
		{
			_innerHost = null;
			await DisposeHostAsync(host).ConfigureAwait(true);
		}

		if (_anonymousHost is { } anonymousHost)
		{
			_anonymousHost = null;
			await DisposeHostAsync(anonymousHost).ConfigureAwait(true);
		}
	}

	/// <inheritdoc />
	public IShodanClient? Client { get; private set; }

	/// <inheritdoc />
	public bool IsConfigured => Client is not null;

	/// <inheritdoc />
	public event Action? Changed;

	/// <inheritdoc />
	public async Task<bool> TryAttachAsync(string apiKey, AppSettings settings,
		CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
		ArgumentNullException.ThrowIfNull(settings);

		var builder = Host.CreateApplicationBuilder();
		builder.Logging.ClearProviders();
		builder.Services.AddShodanClient(options =>
		{
			options.ApiKey = apiKey;
			ApplyOverrides(options, settings);
		});

		var host = builder.Build();
		try
		{
			await host.StartAsync(cancellationToken).ConfigureAwait(true);
			var client = host.Services.GetRequiredService<IShodanClient>();

			// Free-tier call: proves the key is accepted without spending query/scan credits.
			await client.ApiInfo.GetAsync(cancellationToken).ConfigureAwait(true);

			var previousHost = _innerHost;
			_innerHost = host;
			Client = client;

			if (previousHost is not null)
			{
				await DisposeHostAsync(previousHost).ConfigureAwait(true);
			}

			Changed?.Invoke();
			return true;
		}
		catch (Exception ex) when (ex is OptionsValidationException or ShodanException)
		{
			await DisposeHostAsync(host).ConfigureAwait(true);
			return false;
		}
	}

	/// <inheritdoc />
	public async Task DetachAsync()
	{
		var host = _innerHost;
		_innerHost = null;
		Client = null;

		if (host is not null)
		{
			await DisposeHostAsync(host).ConfigureAwait(true);
		}

		Changed?.Invoke();
	}

	/// <inheritdoc />
	public async Task<IShodanClient> GetAnonymousClientAsync(CancellationToken cancellationToken = default)
	{
		if (_anonymousClient is { } existing)
		{
			return existing;
		}

		await _anonymousGate.WaitAsync(cancellationToken).ConfigureAwait(true);
		try
		{
			if (_anonymousClient is { } alreadyBuilt)
			{
				return alreadyBuilt;
			}

			var builder = Host.CreateApplicationBuilder();
			builder.Logging.ClearProviders();
			builder.Services.AddShodanClient(options => options.ApiKey = AnonymousPlaceholderApiKey);

			var host = builder.Build();
			await host.StartAsync(cancellationToken).ConfigureAwait(true);

			_anonymousHost = host;
			_anonymousClient = host.Services.GetRequiredService<IShodanClient>();
			return _anonymousClient;
		}
		finally
		{
			_anonymousGate.Release();
		}
	}

	private static void ApplyOverrides(ShodanClientOptions options, AppSettings settings)
	{
		if (settings.RateLimit is { } rateLimit)
		{
			if (rateLimit.Enabled is { } enabled)
			{
				options.RateLimit.Enabled = enabled;
			}

			if (rateLimit.PermitsPerSecond is { } permitsPerSecond)
			{
				options.RateLimit.PermitsPerSecond = permitsPerSecond;
			}

			if (rateLimit.Burst is { } burst)
			{
				options.RateLimit.Burst = burst;
			}
		}

		if (settings.Resilience is { } resilience)
		{
			if (resilience.MaxRetries is { } maxRetries)
			{
				options.Resilience.MaxRetries = maxRetries;
			}

			if (resilience.BaseDelaySeconds is { } baseDelay)
			{
				options.Resilience.BaseDelay = TimeSpan.FromSeconds(baseDelay);
			}

			if (resilience.AttemptTimeoutSeconds is { } attemptTimeout)
			{
				options.Resilience.AttemptTimeout = TimeSpan.FromSeconds(attemptTimeout);
			}

			if (resilience.TotalTimeoutSeconds is { } totalTimeout)
			{
				options.Resilience.TotalTimeout = TimeSpan.FromSeconds(totalTimeout);
			}
		}

		if (settings.Timeouts is not { } timeouts)
		{
			return;
		}

		if (timeouts.RestSeconds is { } rest)
		{
			options.Timeouts.Rest = TimeSpan.FromSeconds(rest);
		}

		if (timeouts.TrendsSeconds is { } trends)
		{
			options.Timeouts.Trends = TimeSpan.FromSeconds(trends);
		}

		if (timeouts.ExploitsSeconds is { } exploits)
		{
			options.Timeouts.Exploits = TimeSpan.FromSeconds(exploits);
		}

		if (timeouts.InternetDbSeconds is { } internetDb)
		{
			options.Timeouts.InternetDb = TimeSpan.FromSeconds(internetDb);
		}
	}

	private static async Task DisposeHostAsync(IHost host)
	{
		try
		{
			await host.StopAsync().ConfigureAwait(true);
		}
		finally
		{
			host.Dispose();
		}
	}
}
