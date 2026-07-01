using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;
using ShodanClient.Application.Configuration;
using ShodanClient.Application.Exceptions;

namespace ShodanClient.Infrastructure.RateLimiting;

/// <summary>
///     Delegating handler that acquires a rate-limit permit before each request, pacing outgoing
///     traffic to respect Shodan's throughput limit. When the local queue is saturated it surfaces a
///     <see cref="ShodanRateLimitException" /> rather than flooding the server with requests that would
///     be rejected with HTTP 429.
/// </summary>
internal sealed class ShodanRateLimitingHandler(
	IShodanRateLimiterFactory limiterFactory,
	IOptionsMonitor<ShodanClientOptions> options) : DelegatingHandler
{
	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		if (!options.CurrentValue.RateLimit.Enabled)
		{
			return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
		}

		using var lease = await limiterFactory.Limiter
			.AcquireAsync(request, 1, cancellationToken)
			.ConfigureAwait(false);

		if (!lease.IsAcquired)
		{
			var retryAfter = lease.TryGetMetadata(MetadataName.RetryAfter, out var delta) ? delta : (TimeSpan?)null;
			throw new ShodanRateLimitException(
				retryAfter,
				"The client-side rate-limit queue is full; the request was not sent.");
		}

		return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
	}
}
