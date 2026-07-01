using Microsoft.Extensions.DependencyInjection;
using ShodanClient.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace ShodanClient.Tests;

/// <summary>
///     Regression coverage for the BulkData surface: <c>GET /shodan/data</c> and
///     <c>GET /shodan/data/{dataset}</c> both return a bare top-level JSON array, not a
///     <c>{ "matches": [...] }</c> wrapper. The wire DTOs originally modeled the wrapper shape, which
///     made every real call throw a <see cref="ShodanClient.Application.Exceptions.ShodanSerializationException" />.
/// </summary>
public sealed class BulkDataIntegrationTests
{
	[Fact]
	public async Task BulkData_ListDatasetsAsync_maps_the_bare_array_response()
	{
		using var server = WireMockServer.Start();
		const string json = """
		                    [
		                      { "scope": "monthly", "name": "internetdb", "description": "Minified database containing network information about all IPs on the Internet" },
		                      { "scope": "daily", "name": "raw-daily", "description": "Data files containing all the information collected during a day" }
		                    ]
		                    """;
		server
			.Given(Request.Create().WithPath("/shodan/data").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var datasets = await client.BulkData.ListDatasetsAsync(TestContext.Current.CancellationToken);

		Assert.Equal(2, datasets.Count);
		Assert.Equal("monthly", datasets[0].Scope);
		Assert.Equal("internetdb", datasets[0].Name);
		Assert.Equal("daily", datasets[1].Scope);
	}

	[Fact]
	public async Task BulkData_ListFilesAsync_maps_the_bare_array_response()
	{
		using var server = WireMockServer.Start();
		const string json = """
		                    [
		                      { "url": "https://example.com/internetdb.tar.bz2", "timestamp": "2024-09-29T09:39:45.813661", "sha1": "abc123", "name": "internetdb.tar.bz2", "size": 12345 }
		                    ]
		                    """;
		server
			.Given(Request.Create().WithPath("/shodan/data/internetdb").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var files = await client.BulkData.ListFilesAsync("internetdb", TestContext.Current.CancellationToken);

		var file = Assert.Single(files);
		Assert.Equal("internetdb.tar.bz2", file.Name);
		Assert.Equal("abc123", file.Sha1);
		Assert.Equal(12345, file.Size);
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
