using System.ComponentModel.DataAnnotations;

namespace ShodanClient.Application.Configuration;

/// <summary>
///     Client-side rate limiting. Shodan's REST API is throttled to roughly one request per second;
///     the client paces outgoing requests to stay under that limit instead of letting the server
///     reject bursts with HTTP 429. By default requests queue and pace rather than fail.
/// </summary>
public sealed class ShodanRateLimitOptions
{
	/// <summary>Whether client-side rate limiting is enabled (default <see langword="true" />).</summary>
	public bool Enabled { get; set; } = true;

	/// <summary>Tokens replenished each second — the sustained request rate (default 1).</summary>
	[Range(1, int.MaxValue)]
	public int PermitsPerSecond { get; set; } = 1;

	/// <summary>Maximum burst size, i.e. the token bucket capacity (default 1 — no burst).</summary>
	[Range(1, int.MaxValue)]
	public int Burst { get; set; } = 1;

	/// <summary>
	///     Maximum number of requests allowed to wait for a token. Defaults to effectively unbounded
	///     so callers are paced (delayed) rather than rejected. Lower it to fail fast under overload.
	/// </summary>
	[Range(0, int.MaxValue)]
	public int QueueLimit { get; set; } = int.MaxValue;
}
