using Microsoft.Extensions.DependencyInjection;
using ShodanClient.Application.Exceptions;
using ShodanClient.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace ShodanClient.Tests;

/// <summary>
///     End-to-end tests that exercise the whole vertical slice (DI → facade → service → repository →
///     channel → JSON source-gen → domain mapping) against a stubbed HTTP server.
/// </summary>
public sealed class SearchIntegrationTests
{
	[Fact]
	public async Task Hosts_GetAsync_maps_the_host_graph()
	{
		using var server = WireMockServer.Start();
		const string json = """
		                    {
		                      "ip_str": "8.8.8.8",
		                      "ip": 134744072,
		                      "ports": [53, 443],
		                      "hostnames": ["dns.google"],
		                      "country_code": "US",
		                      "org": "Google LLC",
		                      "last_update": "2024-09-29T09:39:45.813661",
		                      "data": [
		                        {
		                          "ip_str": "8.8.8.8",
		                          "port": 53,
		                          "transport": "udp",
		                          "_shodan": { "module": "dns-udp", "ptr": true },
		                          "location": { "country_code": "US", "city": "Mountain View" }
		                        }
		                      ]
		                    }
		                    """;
		server
			.Given(Request.Create().WithPath("/shodan/host/8.8.8.8").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var host = await client.Hosts.GetAsync("8.8.8.8", cancellationToken: TestContext.Current.CancellationToken);

		Assert.Equal("8.8.8.8", host.IpString);
		Assert.Equal([53, 443], host.Ports);
		Assert.Contains("dns.google", host.Hostnames);
		Assert.Equal("US", host.Location?.CountryCode);
		Assert.NotNull(host.LastUpdate);
		Assert.Equal(new DateOnly(2024, 9, 29), DateOnly.FromDateTime(host.LastUpdate.Value.UtcDateTime));
		Assert.Equal(TimeSpan.Zero, host.LastUpdate.Value.Offset);
		var service = Assert.Single(host.Services);
		Assert.Equal(53, service.Port);
		Assert.Equal("udp", service.Transport);
		Assert.True(service.Metadata?.HostnamesFromReverseDns);
	}

	[Fact]
	public async Task Search_SearchAsync_maps_matches_facets_and_total()
	{
		using var server = WireMockServer.Start();
		const string json = """
		                    {
		                      "matches": [
		                        { "ip_str": "1.1.1.1", "port": 80, "transport": "tcp", "http": { "status": 200, "title": "Example" } }
		                      ],
		                      "total": 1,
		                      "facets": { "country": [ { "count": 5, "value": "US" } ] }
		                    }
		                    """;
		server
			.Given(Request.Create().WithPath("/shodan/host/search").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var result = await client.Search.SearchAsync("port:80", "country",
			cancellationToken: TestContext.Current.CancellationToken);

		Assert.Equal(1, result.Total);
		var match = Assert.Single(result.Matches);
		Assert.Equal(200, match.Http?.Status);
		Assert.Equal("Example", match.Http?.Title);
		Assert.Equal("US", result.Facets["country"][0].Value);
		Assert.Equal(5, result.Facets["country"][0].Count);
	}

	[Fact]
	public async Task Hosts_GetAsync_throws_not_found_on_404()
	{
		using var server = WireMockServer.Start();
		server
			.Given(Request.Create().WithPath("/shodan/host/9.9.9.9").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(404)
				.WithHeader("Content-Type", "application/json")
				.WithBody("""{ "error": "No information available for that IP." }"""));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var exception = await Assert.ThrowsAsync<ShodanNotFoundException>(() =>
			client.Hosts.GetAsync("9.9.9.9", cancellationToken: TestContext.Current.CancellationToken));

		Assert.Equal("No information available for that IP.", exception.ApiMessage);
	}

	[Fact]
	public async Task Hosts_GetAsync_validates_ip_before_calling_the_api()
	{
		await using var provider = BuildProvider("http://localhost:1/");
		var client = provider.GetRequiredService<IShodanClient>();

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			client.Hosts.GetAsync("not-an-ip", cancellationToken: TestContext.Current.CancellationToken));
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
