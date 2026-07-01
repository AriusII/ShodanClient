using System.ComponentModel.DataAnnotations;

namespace ShodanClient.Application.Configuration;

/// <summary>
///     Resilience settings applied to non-streaming surfaces via the standard resilience handler
///     (retry with exponential backoff and jitter, circuit breaker, and timeouts). The streaming
///     surface deliberately opts out so long-lived connections are never aborted.
/// </summary>
/// <remarks>
///     The <see cref="TimeSpan" /> properties are deliberately NOT annotated with
///     <c>[Range(typeof(TimeSpan), ...)]</c>: that constructor overload is annotated
///     <c>[RequiresUnreferencedCode]</c> (it needs a <see cref="System.ComponentModel.TypeConverter" />
///     to parse the bounds), which would make this AOT/trim-safe library emit an IL2026 trim warning.
/// </remarks>
public sealed class ShodanResilienceOptions
{
	/// <summary>Maximum retry attempts for transient failures (default 3).</summary>
	[Range(0, 10)]
	public int MaxRetries { get; set; } = 3;

	/// <summary>Base delay for the exponential backoff between retries (default 2s).</summary>
	public TimeSpan BaseDelay { get; set; } = TimeSpan.FromSeconds(2);

	/// <summary>Timeout applied to each individual attempt (default 10s).</summary>
	public TimeSpan AttemptTimeout { get; set; } = TimeSpan.FromSeconds(10);

	/// <summary>Total timeout across all attempts for a single logical request (default 30s).</summary>
	public TimeSpan TotalTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
