using Microsoft.Extensions.DependencyInjection;
using ShodanClient.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace ShodanClient.Tests;

/// <summary>
///     InternetDB is the one surface that requires no API key — these tests confirm the lookup works
///     and, crucially, that the key-appending handler is NOT attached to this client.
/// </summary>
public sealed class InternetDbIntegrationTests
{
	[Fact]
	public async Task InternetDb_GetAsync_maps_the_response_and_sends_no_api_key()
	{
		using var server = WireMockServer.Start();
		const string json = """
		                    { "ip": "8.8.8.8", "ports": [53, 443], "cpes": [], "hostnames": ["dns.google"], "tags": [], "vulns": [] }
		                    """;
		server
			.Given(Request.Create().WithPath("/8.8.8.8").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		var services = new ServiceCollection();
		services.AddShodanClient(options =>
		{
			options.ApiKey = "test-key";
			options.Endpoints.InternetDbBaseUrl = new Uri(server.Url!.EndsWith('/') ? server.Url : server.Url + "/");
		});
		await using var provider = services.BuildServiceProvider();
		var client = provider.GetRequiredService<IShodanClient>();

		var result = await client.InternetDb.GetAsync("8.8.8.8", TestContext.Current.CancellationToken);

		Assert.Equal("8.8.8.8", result.Ip);
		Assert.Equal([53, 443], result.Ports);
		Assert.Contains("dns.google", result.Hostnames);

		var logEntry = Assert.Single(server.LogEntries);
		var requestedUrl = logEntry.RequestMessage?.Url ?? string.Empty;
		Assert.DoesNotContain("key=", requestedUrl, StringComparison.OrdinalIgnoreCase);
	}
}
