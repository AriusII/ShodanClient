using Microsoft.Extensions.DependencyInjection;
using ShodanClient.Application.Exceptions;
using ShodanClient.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace ShodanClient.Tests;

/// <summary>
///     End-to-end tests that exercise the whole vertical slice (DI → facade → service → repository →
///     channel → JSON source-gen → domain mapping) against a stubbed HTTP server for the Trends
///     surface (<c>https://trends.shodan.io</c>).
/// </summary>
public sealed class TrendsIntegrationTests
{
	[Fact]
	public async Task Trends_SearchAsync_maps_monthly_matches_and_facets()
	{
		using var server = WireMockServer.Start();
		const string json = """
		                    {
		                      "total": 42,
		                      "matches": [
		                        { "month": "2024-08", "count": 20 },
		                        { "month": "2024-09", "count": 22 }
		                      ],
		                      "facets": {
		                        "country": [
		                          {
		                            "key": "2024-09",
		                            "values": [
		                              { "value": "US", "count": 15 },
		                              { "value": "DE", "count": 7 }
		                            ]
		                          }
		                        ]
		                      }
		                    }
		                    """;
		server
			.Given(Request.Create().WithPath("/api/v1/search").UsingGet()
				.WithParam("query", "apache").WithParam("facets", "country"))
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var result = await client.Trends.SearchAsync("apache", "country",
			TestContext.Current.CancellationToken);

		Assert.Equal(42, result.Total);
		Assert.Equal(2, result.Matches.Count);
		Assert.Equal("2024-08", result.Matches[0].Month);
		Assert.Equal(20, result.Matches[0].Count);
		Assert.Equal("2024-09", result.Matches[1].Month);
		Assert.Equal(22, result.Matches[1].Count);

		var countryFacet = Assert.Single(result.Facets["country"]);
		Assert.Equal("2024-09", countryFacet.Key);
		Assert.Equal(2, countryFacet.Values.Count);
		Assert.Equal("US", countryFacet.Values[0].Value);
		Assert.Equal(15, countryFacet.Values[0].Count);
		Assert.Equal("DE", countryFacet.Values[1].Value);
		Assert.Equal(7, countryFacet.Values[1].Count);
	}

	[Fact]
	public async Task Trends_GetFiltersAsync_maps_the_string_array()
	{
		using var server = WireMockServer.Start();
		const string json = """["country", "org", "port", "product"]""";
		server
			.Given(Request.Create().WithPath("/api/v1/search/filters").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var filters = await client.Trends.GetFiltersAsync(TestContext.Current.CancellationToken);

		Assert.Equal(["country", "org", "port", "product"], filters);
	}

	[Fact]
	public async Task Trends_GetFacetsAsync_maps_the_string_array()
	{
		using var server = WireMockServer.Start();
		const string json = """["country", "org"]""";
		server
			.Given(Request.Create().WithPath("/api/v1/search/facets").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var facets = await client.Trends.GetFacetsAsync(TestContext.Current.CancellationToken);

		Assert.Equal(["country", "org"], facets);
	}

	[Fact]
	public async Task Trends_SearchAsync_throws_authentication_exception_on_401()
	{
		using var server = WireMockServer.Start();
		server
			.Given(Request.Create().WithPath("/api/v1/search").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(401)
				.WithHeader("Content-Type", "application/json")
				.WithBody("""{ "error": "Invalid API key" }"""));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var exception = await Assert.ThrowsAsync<ShodanAuthenticationException>(() =>
			client.Trends.SearchAsync("apache", cancellationToken: TestContext.Current.CancellationToken));

		Assert.Equal("Invalid API key", exception.ApiMessage);
	}

	private static ServiceProvider BuildProvider(string baseUrl)
	{
		var services = new ServiceCollection();
		services.AddShodanClient(options =>
		{
			options.ApiKey = "test-key";
			options.Endpoints.TrendsBaseUrl = new Uri(baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/");
			options.RateLimit.Enabled = false;
		});
		return services.BuildServiceProvider();
	}
}
