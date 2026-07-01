using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.ExceptionSummarization;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using ShodanClient.Application.Configuration;
using ShodanClient.Infrastructure.Authentication;
using ShodanClient.Infrastructure.Http;
using ShodanClient.Infrastructure.RateLimiting;
using ShodanClient.Infrastructure.Resilience;

namespace ShodanClient.Infrastructure.DependencyInjection;

/// <summary>
///     Registers one typed <see cref="HttpClient" /> per Shodan surface, each with the correct base
///     address, timeout, authentication, rate limiting and resilience. This is where the credential
///     matrix from the design is realized: the API-key handler is attached to every surface except
///     InternetDB; the streaming client opts out of resilience and uses an infinite timeout.
/// </summary>
internal static class ShodanHttpClientRegistration
{
	private static readonly TimeSpan PooledConnectionLifetime = TimeSpan.FromMinutes(2);

	private static SocketsHttpHandler CreateStandardPrimaryHandler() =>
		new()
		{
			PooledConnectionLifetime = PooledConnectionLifetime,
			AutomaticDecompression = DecompressionMethods.All
		};

	private static SocketsHttpHandler CreateNonRedirectingPrimaryHandler() =>
		new()
		{
			PooledConnectionLifetime = PooledConnectionLifetime,
			AutomaticDecompression = DecompressionMethods.All,
			AllowAutoRedirect = false
		};

	private static SocketsHttpHandler CreateStreamingPrimaryHandler() =>
		new()
		{
			PooledConnectionLifetime = PooledConnectionLifetime,
			ConnectTimeout = TimeSpan.FromSeconds(15),
			KeepAlivePingDelay = TimeSpan.FromSeconds(30),
			KeepAlivePingTimeout = TimeSpan.FromSeconds(15),
			AutomaticDecompression = DecompressionMethods.All
		};

	private static ShodanClientOptions GetOptions(IServiceProvider serviceProvider) =>
		serviceProvider.GetRequiredService<IOptions<ShodanClientOptions>>().Value;

	private static Uri ResolveBaseAddress(ShodanApiSurface surface, Uri? overrideBaseUrl) =>
		overrideBaseUrl ?? ShodanSurfaceRegistry.For(surface).DefaultBaseAddress;

	extension(IServiceCollection services)
	{
		public IServiceCollection AddShodanChannels()
		{
			services.TryAddSingleton<IApiKeyProvider, ApiKeyProvider>();
			services.TryAddTransient<ShodanApiKeyHandler>();
			services.TryAddSingleton<IShodanRateLimiterFactory, ShodanRateLimiterFactory>();
			services.TryAddTransient<ShodanRateLimitingHandler>();

			// Enriches resilience telemetry (retry/circuit-breaker/timeout events) with a low-cardinality
			// exception summary plus request/dependency name, so the five typed HttpClients registered
			// below can be told apart on a dashboard instead of emitting Polly's generic event shape.
			services.AddResilienceEnricher();
			services.AddExceptionSummarizer(static b => b.AddHttpProvider());

			services.AddRestClient();
			services.AddStreamingClient();
			services.AddTrendsClient();
			services.AddInternetDbClient();
			services.AddExploitsClient();
			return services;
		}

		private void AddRestClient()
		{
			var builder = services.AddHttpClient<RestChannel>((sp, client) =>
			{
				var options = GetOptions(sp);
				client.BaseAddress = ResolveBaseAddress(ShodanApiSurface.Rest, options.Endpoints.RestBaseUrl);
				client.Timeout = options.Timeouts.Rest;
			});

			builder.ConfigurePrimaryHttpMessageHandler(CreateStandardPrimaryHandler);
			builder.AddHttpMessageHandler<ShodanRateLimitingHandler>();
			// The key handler MUST run outside (before) resilience: AddHttpMessageHandler pipelines
			// are outermost-added-first, and the standard resilience handler resends the SAME
			// HttpRequestMessage instance on each retry attempt. Attaching the key handler downstream
			// of resilience would re-append "&key=..." on every retry, growing the query string
			// without bound.
			builder.AddHttpMessageHandler<ShodanApiKeyHandler>();
			services.ApplyStandardResilience(builder);
		}

		private void AddTrendsClient()
		{
			var builder = services.AddHttpClient<TrendsChannel>((sp, client) =>
			{
				var options = GetOptions(sp);
				client.BaseAddress = ResolveBaseAddress(ShodanApiSurface.Trends, options.Endpoints.TrendsBaseUrl);
				client.Timeout = options.Timeouts.Trends;
			});

			builder.ConfigurePrimaryHttpMessageHandler(CreateStandardPrimaryHandler);
			builder.AddHttpMessageHandler<ShodanRateLimitingHandler>();
			builder.AddHttpMessageHandler<ShodanApiKeyHandler>();
			services.ApplyStandardResilience(builder);
		}

		private void AddExploitsClient()
		{
			var builder = services.AddHttpClient<ExploitsChannel>((sp, client) =>
			{
				var options = GetOptions(sp);
				client.BaseAddress = ResolveBaseAddress(ShodanApiSurface.Exploits, options.Endpoints.ExploitsBaseUrl);
				client.Timeout = options.Timeouts.Exploits;
			});

			// The Exploits API's documented host (exploits.shodan.io) currently 301-redirects to
			// cvedb.shodan.io, an entirely different, incompatible schema. Auto-redirect is disabled so
			// ShodanChannel observes the raw 301 and fails loudly and clearly instead of silently
			// deserializing the redirect target's response as if it were an exploit search result.
			builder.ConfigurePrimaryHttpMessageHandler(CreateNonRedirectingPrimaryHandler);
			builder.AddHttpMessageHandler<ShodanRateLimitingHandler>();
			builder.AddHttpMessageHandler<ShodanApiKeyHandler>();
			services.ApplyStandardResilience(builder);
		}

		private void AddInternetDbClient()
		{
			// InternetDB requires NO API key, so the key handler is deliberately not attached.
			var builder = services.AddHttpClient<InternetDbChannel>((sp, client) =>
			{
				var options = GetOptions(sp);
				client.BaseAddress =
					ResolveBaseAddress(ShodanApiSurface.InternetDb, options.Endpoints.InternetDbBaseUrl);
				client.Timeout = options.Timeouts.InternetDb;
			});

			builder.ConfigurePrimaryHttpMessageHandler(CreateStandardPrimaryHandler);
			services.ApplyStandardResilience(builder);
		}

		private void AddStreamingClient()
		{
			// Long-lived NDJSON connections: infinite timeout, no resilience, no rate limiting.
			var builder = services.AddHttpClient<StreamingChannel>((sp, client) =>
			{
				var options = GetOptions(sp);
				client.BaseAddress = ResolveBaseAddress(ShodanApiSurface.Streaming, options.Endpoints.StreamingBaseUrl);
				client.Timeout = Timeout.InfiniteTimeSpan;
			});

			builder.ConfigurePrimaryHttpMessageHandler(CreateStreamingPrimaryHandler);
			builder.AddHttpMessageHandler<ShodanApiKeyHandler>();
		}

		private void ApplyStandardResilience(IHttpClientBuilder builder)
		{
			var pipelineBuilder = builder.AddStandardResilienceHandler();

			// Bind the pipeline's options to ShodanClientOptions.Resilience through the options system
			// so user overrides are honored (there is no IServiceProvider-based Configure overload here).
			services.AddOptions<HttpStandardResilienceOptions>(pipelineBuilder.PipelineName)
				.Configure<IOptions<ShodanClientOptions>>(static (resilience, shodan) =>
					ShodanResiliencePipelines.ConfigureStandard(resilience, shodan.Value.Resilience));
		}
	}
}
