namespace ShodanClient.App.Services.Settings;

/// <summary>
///     Non-sensitive local app preferences persisted as plain JSON. The Shodan API key itself never
///     lives here — see <see cref="ShodanClient.App.Services.Credentials.ICredentialStore" />.
/// </summary>
public sealed class AppSettings
{
	/// <summary>The preferred visual theme.</summary>
	public ThemePreference Theme { get; set; } = ThemePreference.Dark;

	/// <summary>Last known main window width.</summary>
	public double WindowWidth { get; set; } = 1280;

	/// <summary>Last known main window height.</summary>
	public double WindowHeight { get; set; } = 800;

	/// <summary>Optional override for the client-side rate limit applied to REST calls.</summary>
	public RateLimitOverride? RateLimit { get; set; }

	/// <summary>Optional override for the resilience (retry/circuit-breaker/timeout) pipeline.</summary>
	public ResilienceOverride? Resilience { get; set; }

	/// <summary>Optional override for per-surface request timeouts.</summary>
	public TimeoutOverride? Timeouts { get; set; }
}

/// <summary>
///     Optional override mirroring <see cref="ShodanClient.Application.Configuration.ShodanRateLimitOptions" />.
///     Any <see langword="null" /> property falls back to the SDK default.
/// </summary>
public sealed class RateLimitOverride
{
	/// <summary>See <c>ShodanRateLimitOptions.Enabled</c>.</summary>
	public bool? Enabled { get; set; }

	/// <summary>See <c>ShodanRateLimitOptions.PermitsPerSecond</c>.</summary>
	public int? PermitsPerSecond { get; set; }

	/// <summary>See <c>ShodanRateLimitOptions.Burst</c>.</summary>
	public int? Burst { get; set; }
}

/// <summary>
///     Optional override mirroring <see cref="ShodanClient.Application.Configuration.ShodanResilienceOptions" />.
///     Any <see langword="null" /> property falls back to the SDK default.
/// </summary>
public sealed class ResilienceOverride
{
	/// <summary>See <c>ShodanResilienceOptions.MaxRetries</c>.</summary>
	public int? MaxRetries { get; set; }

	/// <summary>See <c>ShodanResilienceOptions.BaseDelay</c>, in seconds.</summary>
	public double? BaseDelaySeconds { get; set; }

	/// <summary>See <c>ShodanResilienceOptions.AttemptTimeout</c>, in seconds.</summary>
	public double? AttemptTimeoutSeconds { get; set; }

	/// <summary>See <c>ShodanResilienceOptions.TotalTimeout</c>, in seconds.</summary>
	public double? TotalTimeoutSeconds { get; set; }
}

/// <summary>
///     Optional override mirroring <see cref="ShodanClient.Application.Configuration.ShodanTimeoutOptions" />.
///     Any <see langword="null" /> property falls back to the SDK default. Values are in seconds.
/// </summary>
public sealed class TimeoutOverride
{
	/// <summary>See <c>ShodanTimeoutOptions.Rest</c>.</summary>
	public double? RestSeconds { get; set; }

	/// <summary>See <c>ShodanTimeoutOptions.Trends</c>.</summary>
	public double? TrendsSeconds { get; set; }

	/// <summary>See <c>ShodanTimeoutOptions.Exploits</c>.</summary>
	public double? ExploitsSeconds { get; set; }

	/// <summary>See <c>ShodanTimeoutOptions.InternetDb</c>.</summary>
	public double? InternetDbSeconds { get; set; }
}
