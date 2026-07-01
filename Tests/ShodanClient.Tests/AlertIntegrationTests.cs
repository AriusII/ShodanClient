using Microsoft.Extensions.DependencyInjection;
using ShodanClient.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace ShodanClient.Tests;

/// <summary>
///     Regression coverage for the Alerts surface: <c>GET /shodan/alert/triggers</c> returns a JSON
///     array of <c>{ name, rule, description }</c> objects, not an object keyed by trigger name. The
///     original wire DTO deserialized it as a <c>Dictionary&lt;string, TriggerDefinitionDto&gt;</c>,
///     which throws a <see cref="System.Text.Json.JsonException" /> against the real array response.
/// </summary>
public sealed class AlertIntegrationTests
{
	[Fact]
	public async Task Alerts_GetTriggersAsync_maps_the_array_response()
	{
		using var server = WireMockServer.Start();
		const string json = """
		                    [
		                      { "name": "any", "rule": "*", "description": "Match any service that is discovered" },
		                      { "name": "malware", "rule": "tag:compromised,malware", "description": "Compromised or malware-related services" }
		                    ]
		                    """;
		server
			.Given(Request.Create().WithPath("/shodan/alert/triggers").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var triggers = await client.Alerts.GetTriggersAsync(TestContext.Current.CancellationToken);

		Assert.Equal(2, triggers.Count);
		Assert.Equal("any", triggers[0].Name);
		Assert.Equal("*", triggers[0].Rule);
		Assert.Equal("malware", triggers[1].Name);
	}

	[Fact]
	public async Task Alerts_GetAsync_maps_has_triggers_and_expiration()
	{
		using var server = WireMockServer.Start();
		const string json = """
		                    {
		                      "id": "alert-1",
		                      "name": "test-alert",
		                      "created": "2024-09-29T09:39:45.813661",
		                      "expires": 0,
		                      "size": 1,
		                      "filters": { "ip": ["1.1.1.1"] },
		                      "triggers": {},
		                      "has_triggers": true,
		                      "expiration": "2025-01-01T00:00:00.000000",
		                      "notifiers": []
		                    }
		                    """;
		server
			.Given(Request.Create().WithPath("/shodan/alert/alert-1/info").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var alert = await client.Alerts.GetAsync("alert-1", TestContext.Current.CancellationToken);

		Assert.True(alert.HasTriggers);
		Assert.Equal("2025-01-01T00:00:00.000000", alert.Expiration);
	}

	private static ServiceProvider BuildProvider(string baseUrl)
	{
		var services = new ServiceCollection();
		services.AddShodanClient(options =>
		{
			options.ApiKey = "test-key";
			options.Endpoints.RestBaseUrl = new Uri(baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/");
			options.RateLimit.Enabled = false;
		});
		return services.BuildServiceProvider();
	}
}
