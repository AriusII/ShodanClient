using Microsoft.Extensions.Http.Resilience;
using ShodanClient.Application.Configuration;
using ShodanClient.Infrastructure.Resilience;

namespace ShodanClient.Infrastructure.Tests;

/// <summary>
///     Unit coverage for <see cref="ShodanResiliencePipelines.ConfigureStandard" />: verifies the
///     retry, timeout and circuit-breaker settings on a <see cref="HttpStandardResilienceOptions" />
///     pipeline are wired from <see cref="ShodanResilienceOptions" />, and that the circuit breaker's
///     minimum-throughput floor is applied regardless of the settings passed in.
/// </summary>
public sealed class ShodanResiliencePipelinesTests
{
	[Fact]
	public void ConfigureStandard_with_default_settings_sets_minimum_throughput_to_the_floor()
	{
		var pipeline = new HttpStandardResilienceOptions();
		var settings = new ShodanResilienceOptions();

		ShodanResiliencePipelines.ConfigureStandard(pipeline, settings);

		Assert.Equal(8, pipeline.CircuitBreaker.MinimumThroughput);
	}

	[Fact]
	public void ConfigureStandard_wires_max_retry_attempts_from_settings()
	{
		var pipeline = new HttpStandardResilienceOptions();
		var settings = new ShodanResilienceOptions { MaxRetries = 5 };

		ShodanResiliencePipelines.ConfigureStandard(pipeline, settings);

		Assert.Equal(5, pipeline.Retry.MaxRetryAttempts);
	}

	[Fact]
	public void ConfigureStandard_wires_delay_and_timeouts_from_settings()
	{
		var pipeline = new HttpStandardResilienceOptions();
		var settings = new ShodanResilienceOptions
		{
			MaxRetries = 4,
			BaseDelay = TimeSpan.FromSeconds(3),
			AttemptTimeout = TimeSpan.FromSeconds(15),
			TotalTimeout = TimeSpan.FromSeconds(45)
		};

		ShodanResiliencePipelines.ConfigureStandard(pipeline, settings);

		Assert.Equal(settings.BaseDelay, pipeline.Retry.Delay);
		Assert.Equal(settings.AttemptTimeout, pipeline.AttemptTimeout.Timeout);
		Assert.Equal(settings.TotalTimeout, pipeline.TotalRequestTimeout.Timeout);
	}

	[Fact]
	public void ConfigureStandard_raises_sampling_duration_to_at_least_twice_the_attempt_timeout()
	{
		var pipeline = new HttpStandardResilienceOptions();
		var settings = new ShodanResilienceOptions { AttemptTimeout = TimeSpan.FromSeconds(15) };

		ShodanResiliencePipelines.ConfigureStandard(pipeline, settings);

		Assert.True(
			pipeline.CircuitBreaker.SamplingDuration >= TimeSpan.FromTicks(settings.AttemptTimeout.Ticks * 2));
	}

	[Fact]
	public void ConfigureStandard_minimum_throughput_floor_is_independent_of_settings_values()
	{
		var pipeline = new HttpStandardResilienceOptions();
		var settings = new ShodanResilienceOptions
		{
			MaxRetries = 0,
			BaseDelay = TimeSpan.FromMilliseconds(1),
			AttemptTimeout = TimeSpan.FromMinutes(5),
			TotalTimeout = TimeSpan.FromMinutes(10)
		};

		ShodanResiliencePipelines.ConfigureStandard(pipeline, settings);

		Assert.Equal(8, pipeline.CircuitBreaker.MinimumThroughput);
	}
}
