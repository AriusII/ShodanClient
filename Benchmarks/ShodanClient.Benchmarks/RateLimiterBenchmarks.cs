using System.Threading.RateLimiting;
using BenchmarkDotNet.Attributes;

namespace ShodanClient.Benchmarks;

/// <summary>
///     Compares the cost of the synchronous <see cref="PartitionedRateLimiter{TResource}.AttemptAcquire" />
///     fast path (a permit is always immediately available) against the added cost of disposing the
///     returned <see cref="RateLimitLease" />. The limiter is configured exactly like the production
///     <c>ShodanRateLimiterFactory</c> (see
///     Src/ShodanClient.Infrastructure/RateLimiting/ShodanRateLimiterFactory.cs): a single-partition token
///     bucket, replenished once per second, oldest-first queue ordering, and
///     <see cref="TokenBucketRateLimiterOptions.AutoReplenishment" /> disabled (the shared limiter relies on
///     the partitioned limiter's own replenishment timer rather than a per-partition one). Unlike
///     production, <see cref="TokenLimit" /> is inflated far past the real default of 1 so an entire
///     benchmark iteration's worth of acquisitions succeeds without the bucket running dry -- token-bucket
///     leases never return their token on <see cref="RateLimitLease.Dispose()" />, and with
///     AutoReplenishment off nothing else refills the bucket mid-run. A fresh limiter is also rebuilt every
///     iteration via <see cref="IterationSetup" /> as a safety net against exhaustion on long-running jobs.
/// </summary>
[MemoryDiagnoser]
public class RateLimiterBenchmarks
{
	private const string PartitionKey = "api.shodan.io";

	// Mirrors ShodanRateLimiterFactory.Build's TokenBucketRateLimiterOptions, except TokenLimit, which is
	// inflated well past production's default of 1 so a full benchmark iteration of AttemptAcquire calls
	// succeeds without the (AutoReplenishment = false) bucket running dry.
	private const int TokenLimit = 50_000_000;
	private const int TokensPerPeriod = 1;
	private const int QueueLimit = int.MaxValue;
	private static readonly TimeSpan ReplenishmentPeriod = TimeSpan.FromSeconds(1);
	private RateLimitLease? _lastLease;

	private PartitionedRateLimiter<string> _limiter = null!;

	/// <summary>Rebuilds the limiter with a full token bucket before every iteration.</summary>
	[IterationSetup]
	public void IterationSetup()
	{
		_limiter?.Dispose();
		_limiter = CreateLimiter();
	}

	[GlobalCleanup]
	public void GlobalCleanup()
	{
		_lastLease?.Dispose();
		_limiter?.Dispose();
	}

	/// <summary>
	///     Acquires a permit and discards the lease without disposing it, isolating the acquire-only cost
	///     from the cost of disposal measured by <see cref="AttemptAcquireAndDispose" />. The lease is
	///     stashed in an instance field rather than left in an undisposed local so the compiler treats
	///     ownership as transferred (no CA2000) instead of flagging a leak; token-bucket leases hold no
	///     unmanaged resources, so skipping disposal here has no observable effect beyond this benchmark.
	/// </summary>
	[Benchmark(Baseline = true, Description = "AttemptAcquire (no dispose)")]
	public bool AttemptAcquire()
	{
		_lastLease = _limiter.AttemptAcquire(PartitionKey);
		return _lastLease.IsAcquired;
	}

	/// <summary>
	///     The realistic call-site shape used by <c>ShodanRateLimitingHandler</c>: a
	///     <see langword="using" /> statement around the acquired lease, so its <c>Dispose()</c> cost is
	///     included alongside the acquire itself.
	/// </summary>
	[Benchmark(Description = "AttemptAcquire + Dispose (using)")]
	public bool AttemptAcquireAndDispose()
	{
		using var lease = _limiter.AttemptAcquire(PartitionKey);
		return lease.IsAcquired;
	}

	private static PartitionedRateLimiter<string> CreateLimiter() =>
		PartitionedRateLimiter.Create<string, string>(_ =>
			RateLimitPartition.GetTokenBucketLimiter(
				PartitionKey,
				static _ => new TokenBucketRateLimiterOptions
				{
					TokenLimit = TokenLimit,
					TokensPerPeriod = TokensPerPeriod,
					ReplenishmentPeriod = ReplenishmentPeriod,
					QueueLimit = QueueLimit,
					QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
					AutoReplenishment = false
				}));
}
