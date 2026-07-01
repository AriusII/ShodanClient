using Microsoft.Extensions.Http.Resilience;
using Polly;
using ShodanClient.Application.Configuration;

namespace ShodanClient.Infrastructure.Resilience;

/// <summary>
///     Configures the standard resilience pipeline (retry with exponential backoff and jitter, a
///     circuit breaker, per-attempt and total-request timeouts) for the non-streaming Shodan surfaces
///     from <see cref="ShodanResilienceOptions" />. The streaming surface deliberately opts out.
/// </summary>
internal static class ShodanResiliencePipelines
{
	/// <summary>
	///     Circuit-breaker minimum-throughput floor for a client whose realistic sustained rate is
	///     on the order of ~1 request/second (see the comment in <see cref="ConfigureStandard" />).
	/// </summary>
	private const int MinimumThroughputFloor = 8;

	public static void ConfigureStandard(HttpStandardResilienceOptions pipeline, ShodanResilienceOptions settings)
	{
		pipeline.Retry.MaxRetryAttempts = settings.MaxRetries;
		pipeline.Retry.Delay = settings.BaseDelay;
		pipeline.Retry.BackoffType = DelayBackoffType.Exponential;
		pipeline.Retry.UseJitter = true;

		// Never replay POST/PUT/DELETE: several REST routes are non-idempotent (launching a scan,
		// creating an alert/notifier) and retrying them after a lost response risks duplicating the
		// side effect (e.g. a second billed scan). GET/HEAD/OPTIONS keep retrying as normal.
		pipeline.Retry.DisableForUnsafeHttpMethods();

		pipeline.AttemptTimeout.Timeout = settings.AttemptTimeout;
		pipeline.TotalRequestTimeout.Timeout = settings.TotalTimeout;

		// The circuit breaker's sampling window must be at least twice the attempt timeout.
		pipeline.CircuitBreaker.SamplingDuration = TimeSpan.FromTicks(
			Math.Max(pipeline.CircuitBreaker.SamplingDuration.Ticks, settings.AttemptTimeout.Ticks * 2));

		// The library default (100 requests/30s) is tuned for high-volume service-to-service
		// traffic. This client's own rate limiter caps sustained throughput at roughly 1
		// request/second by default (see ShodanRateLimitOptions), so a 100-request minimum would
		// almost never be reached in normal single-consumer usage and the breaker would never trip.
		// A small fixed floor keeps the breaker meaningful for a low-throughput client while still
		// requiring more than one or two failures before it opens.
		pipeline.CircuitBreaker.MinimumThroughput = MinimumThroughputFloor;
	}
}
