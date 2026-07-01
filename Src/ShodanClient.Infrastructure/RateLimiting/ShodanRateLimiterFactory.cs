using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;
using ShodanClient.Application.Configuration;

namespace ShodanClient.Infrastructure.RateLimiting;

/// <summary>
///     Builds and owns the shared token-bucket rate limiter, partitioned by request host, from
///     <see cref="ShodanRateLimitOptions" />. Registered as a singleton so all requests share the
///     same buckets and stay collectively under Shodan's ~1 request/second limit.
/// </summary>
internal sealed class ShodanRateLimiterFactory : IShodanRateLimiterFactory, IDisposable, IAsyncDisposable
{
	public ShodanRateLimiterFactory(IOptions<ShodanClientOptions> options)
	{
		ArgumentNullException.ThrowIfNull(options);
		Limiter = Build(options.Value.RateLimit);
	}

	public ValueTask DisposeAsync() => Limiter.DisposeAsync();

	public void Dispose() => Limiter.Dispose();

	/// <inheritdoc />
	public PartitionedRateLimiter<HttpRequestMessage> Limiter { get; }

	private static PartitionedRateLimiter<HttpRequestMessage> Build(ShodanRateLimitOptions options)
	{
		var tokenLimit = Math.Max(1, options.Burst);
		var tokensPerPeriod = Math.Max(1, options.PermitsPerSecond);
		var queueLimit = Math.Max(0, options.QueueLimit);

		return PartitionedRateLimiter.Create<HttpRequestMessage, string>(request =>
		{
			var partitionKey = request.RequestUri?.Host ?? "shodan";
			return RateLimitPartition.GetTokenBucketLimiter(
				partitionKey,
				_ => new TokenBucketRateLimiterOptions
				{
					TokenLimit = tokenLimit,
					TokensPerPeriod = tokensPerPeriod,
					ReplenishmentPeriod = TimeSpan.FromSeconds(1),
					QueueLimit = queueLimit,
					QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
					// False per RateLimitPartition.GetTokenBucketLimiter's own guidance: the
					// partitioned limiter already runs one shared replenishment timer for every
					// partition, so a per-partition auto-replenishing timer would be redundant.
					AutoReplenishment = false
				});
		});
	}
}
