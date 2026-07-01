using Microsoft.Extensions.DependencyInjection;
using ShodanClient.Application.Exceptions;
using ShodanClient.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace ShodanClient.Tests;

/// <summary>
///     End-to-end tests that exercise the whole vertical slice (DI → facade → service → repository →
///     channel → JSON source-gen → domain mapping) for the Directory (saved query) surface against a
///     stubbed HTTP server.
/// </summary>
public sealed class DirectoryIntegrationTests
{
	[Fact]
	public async Task Directory_ListQueriesAsync_maps_matches_and_total()
	{
		using var server = WireMockServer.Start();
		const string json = """
		                    {
		                      "matches": [
		                        {
		                          "query": "webcam",
		                          "votes": 209,
		                          "title": "Webcams",
		                          "description": "Publicly accessible webcams",
		                          "timestamp": "2024-09-29T09:39:45.813661",
		                          "tags": ["webcam", "video"]
		                        }
		                      ],
		                      "total": 1
		                    }
		                    """;
		server
			.Given(Request.Create().WithPath("/shodan/query").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var result = await client.Directory.ListQueriesAsync(
			cancellationToken: TestContext.Current.CancellationToken);

		Assert.Equal(1, result.Total);
		var match = Assert.Single(result.Matches);
		Assert.Equal("webcam", match.Query);
		Assert.Equal(209, match.Votes);
		Assert.Equal("Webcams", match.Title);
		Assert.Equal("Publicly accessible webcams", match.Description);
		Assert.Equal(["webcam", "video"], match.Tags);
		Assert.NotNull(match.Timestamp);
	}

	[Fact]
	public async Task Directory_SearchQueriesAsync_sends_query_and_maps_matches()
	{
		using var server = WireMockServer.Start();
		const string json = """
		                    {
		                      "matches": [
		                        { "query": "port:3389", "votes": 12, "title": "RDP", "tags": ["rdp"] }
		                      ],
		                      "total": 1
		                    }
		                    """;
		server
			.Given(Request.Create().WithPath("/shodan/query/search").WithParam("query", "rdp").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var result = await client.Directory.SearchQueriesAsync("rdp",
			cancellationToken: TestContext.Current.CancellationToken);

		Assert.Equal(1, result.Total);
		var match = Assert.Single(result.Matches);
		Assert.Equal("port:3389", match.Query);
		Assert.Equal(12, match.Votes);
		Assert.Equal("RDP", match.Title);
	}

	[Fact]
	public async Task Directory_ListTagsAsync_maps_value_and_count()
	{
		using var server = WireMockServer.Start();
		const string json = """
		                    {
		                      "matches": [
		                        { "value": "webcam", "count": 209 },
		                        { "value": "honeypot", "count": 42 }
		                      ],
		                      "total": 2
		                    }
		                    """;
		server
			.Given(Request.Create().WithPath("/shodan/query/tags").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var tags = await client.Directory.ListTagsAsync(cancellationToken: TestContext.Current.CancellationToken);

		Assert.Equal(2, tags.Count);
		Assert.Equal("webcam", tags[0].Value);
		Assert.Equal(209, tags[0].Count);
		Assert.Equal("honeypot", tags[1].Value);
		Assert.Equal(42, tags[1].Count);
	}

	[Fact]
	public async Task Directory_ListQueriesAsync_throws_authentication_exception_on_401()
	{
		using var server = WireMockServer.Start();
		server
			.Given(Request.Create().WithPath("/shodan/query").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(401)
				.WithHeader("Content-Type", "application/json")
				.WithBody("""{ "error": "Invalid API key" }"""));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var exception = await Assert.ThrowsAsync<ShodanAuthenticationException>(() =>
			client.Directory.ListQueriesAsync(cancellationToken: TestContext.Current.CancellationToken));

		Assert.Equal("Invalid API key", exception.ApiMessage);
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
