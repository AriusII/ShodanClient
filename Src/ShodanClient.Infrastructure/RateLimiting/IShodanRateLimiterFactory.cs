using System.Threading.RateLimiting;

namespace ShodanClient.Infrastructure.RateLimiting;

/// <summary>
///     Provides the shared, process-wide rate limiter that paces outgoing Shodan requests. Partitioned
///     by request host so each surface is throttled independently.
/// </summary>
internal interface IShodanRateLimiterFactory
{
	/// <summary>The shared partitioned rate limiter.</summary>
	PartitionedRateLimiter<HttpRequestMessage> Limiter { get; }
}
