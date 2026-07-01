using Microsoft.Extensions.DependencyInjection;
using ShodanClient.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace ShodanClient.Tests;

/// <summary>
///     Regression coverage for two bugs found in the shared HTTP core during a hardening audit:
///     (1) the API-key handler used to sit downstream of the resilience retry pipeline, so each retry
///     re-appended <c>&amp;key=...</c> to the same request, growing the query string without bound;
///     (2) <see cref="ShodanClient.Infrastructure.Http.ShodanChannel.SendForSuccessAsync" /> used to
///     treat a 2xx body that simply omits the <c>success</c> field as a failure, because the wire DTO's
///     <c>Success</c> property defaulted to <see langword="false" /> instead of being nullable.
/// </summary>
public sealed class HttpCoreRegressionTests
{
	[Fact]
	public async Task Retried_request_carries_exactly_one_api_key_parameter()
	{
		using var server = WireMockServer.Start();
		server
			.Given(Request.Create().WithPath("/shodan/host/8.8.8.8").UsingGet())
			.InScenario("retry")
			.WillSetStateTo("second-attempt")
			.RespondWith(Response.Create().WithStatusCode(500));
		server
			.Given(Request.Create().WithPath("/shodan/host/8.8.8.8").UsingGet())
			.InScenario("retry")
			.WhenStateIs("second-attempt")
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json")
				.WithBody("""{ "ip_str": "8.8.8.8", "ports": [] }"""));

		var services = new ServiceCollection();
		services.AddShodanClient(options =>
		{
			options.ApiKey = "test-key";
			options.Endpoints.RestBaseUrl = new Uri(server.Url!.EndsWith('/') ? server.Url : server.Url + "/");
			options.RateLimit.Enabled = false;
			options.Resilience.MaxRetries = 2;
			options.Resilience.BaseDelay = TimeSpan.FromMilliseconds(10);
		});
		await using var provider = services.BuildServiceProvider();
		var client = provider.GetRequiredService<IShodanClient>();

		var host = await client.Hosts.GetAsync("8.8.8.8", cancellationToken: TestContext.Current.CancellationToken);

		Assert.Equal("8.8.8.8", host.IpString);
		Assert.Equal(2, server.LogEntries.Count);
		foreach (var logEntry in server.LogEntries)
		{
			var url = logEntry.RequestMessage?.Url ?? string.Empty;
			var keyOccurrences = url.Split("key=").Length - 1;
			Assert.Equal(1, keyOccurrences);
		}
	}

	[Fact]
	public async Task DeleteAsync_treats_a_2xx_body_without_a_success_field_as_success()
	{
		using var server = WireMockServer.Start();
		server
			.Given(Request.Create().WithPath("/shodan/alert/alert-1").UsingDelete())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody("{}"));

		var services = new ServiceCollection();
		services.AddShodanClient(options =>
		{
			options.ApiKey = "test-key";
			options.Endpoints.RestBaseUrl = new Uri(server.Url!.EndsWith('/') ? server.Url : server.Url + "/");
			options.RateLimit.Enabled = false;
		});
		await using var provider = services.BuildServiceProvider();
		var client = provider.GetRequiredService<IShodanClient>();

		var success = await client.Alerts.DeleteAsync("alert-1", TestContext.Current.CancellationToken);

		Assert.True(success);
	}
}
