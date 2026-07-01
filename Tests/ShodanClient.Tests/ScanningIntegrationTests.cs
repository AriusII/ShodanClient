using Microsoft.Extensions.DependencyInjection;
using ShodanClient.Application.Exceptions;
using ShodanClient.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace ShodanClient.Tests;

/// <summary>
///     End-to-end tests that exercise the whole vertical slice (DI → facade → service → repository →
///     channel → JSON source-gen → domain mapping) for the on-demand Scanning surface against a
///     stubbed HTTP server.
/// </summary>
public sealed class ScanningIntegrationTests
{
	[Fact]
	public async Task Scans_GetCrawledPortsAsync_maps_the_port_list()
	{
		using var server = WireMockServer.Start();
		const string json = "[7, 21, 22, 80, 443, 8080]";
		server
			.Given(Request.Create().WithPath("/shodan/ports").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var ports = await client.Scans.GetCrawledPortsAsync(TestContext.Current.CancellationToken);

		Assert.Equal([7, 21, 22, 80, 443, 8080], ports);
	}

	[Fact]
	public async Task Scans_GetProtocolsAsync_maps_the_protocol_dictionary()
	{
		using var server = WireMockServer.Start();
		const string json = """
		                    {
		                      "telnet": "Telnet",
		                      "https": "HTTPS Crawler"
		                    }
		                    """;
		server
			.Given(Request.Create().WithPath("/shodan/protocols").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var protocols = await client.Scans.GetProtocolsAsync(TestContext.Current.CancellationToken);

		Assert.Equal(2, protocols.Count);
		Assert.Equal("Telnet", protocols["telnet"]);
		Assert.Equal("HTTPS Crawler", protocols["https"]);
	}

	[Fact]
	public async Task Scans_RequestScanAsync_posts_the_ips_form_and_maps_the_submission()
	{
		using var server = WireMockServer.Start();
		const string json = """
		                    {
		                      "id": "scan-123",
		                      "count": 2,
		                      "credits_left": 98
		                    }
		                    """;
		server
			.Given(Request.Create().WithPath("/shodan/scan").UsingPost()
				.WithBody("ips=1.1.1.1%2C2.2.2.2"))
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var submission = await client.Scans.RequestScanAsync(["1.1.1.1", "2.2.2.2"],
			TestContext.Current.CancellationToken);

		Assert.Equal("scan-123", submission.Id);
		Assert.Equal(2, submission.Count);
		Assert.Equal(98, submission.CreditsLeft);
	}

	[Fact]
	public async Task Scans_ScanInternetAsync_posts_the_port_and_protocol_form_and_maps_the_result()
	{
		using var server = WireMockServer.Start();
		const string json = """{ "id": "scan-internet-456" }""";
		server
			.Given(Request.Create().WithPath("/shodan/scan/internet").UsingPost()
				.WithBody("port=443&protocol=https"))
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var result = await client.Scans.ScanInternetAsync(443, "https",
			TestContext.Current.CancellationToken);

		Assert.Equal("scan-internet-456", result.Id);
	}

	[Fact]
	public async Task Scans_ListScansAsync_maps_the_matches_array()
	{
		using var server = WireMockServer.Start();
		const string json = """
		                    {
		                      "matches": [
		                        {
		                          "id": "scan-123",
		                          "created": "2024-09-29T09:39:45.813661",
		                          "status": "PROCESSING",
		                          "size": 2,
		                          "credits_left": 98
		                        }
		                      ],
		                      "total": 1
		                    }
		                    """;
		server
			.Given(Request.Create().WithPath("/shodan/scans").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var scans = await client.Scans.ListScansAsync(TestContext.Current.CancellationToken);

		var scan = Assert.Single(scans);
		Assert.Equal("scan-123", scan.Id);
		Assert.Equal("PROCESSING", scan.Status);
		Assert.Equal(2, scan.Size);
		Assert.Equal(98, scan.CreditsLeft);
		Assert.NotNull(scan.Created);
		Assert.Equal(new DateOnly(2024, 9, 29), DateOnly.FromDateTime(scan.Created!.Value.UtcDateTime));
	}

	[Fact]
	public async Task Scans_GetScanStatusAsync_maps_the_scan_status()
	{
		using var server = WireMockServer.Start();
		const string json = """
		                    {
		                      "id": "scan-123",
		                      "status": "DONE",
		                      "created": "2024-09-29T09:39:45.813661",
		                      "count": 2
		                    }
		                    """;
		server
			.Given(Request.Create().WithPath("/shodan/scans/scan-123").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var status = await client.Scans.GetScanStatusAsync("scan-123", TestContext.Current.CancellationToken);

		Assert.Equal("scan-123", status.Id);
		Assert.Equal("DONE", status.Status);
		Assert.Equal(2, status.Count);
	}

	[Fact]
	public async Task Scans_GetScanStatusAsync_throws_not_found_on_404()
	{
		using var server = WireMockServer.Start();
		server
			.Given(Request.Create().WithPath("/shodan/scans/unknown-scan").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(404)
				.WithHeader("Content-Type", "application/json")
				.WithBody("""{ "error": "Scan not found" }"""));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var exception = await Assert.ThrowsAsync<ShodanNotFoundException>(() =>
			client.Scans.GetScanStatusAsync("unknown-scan", TestContext.Current.CancellationToken));

		Assert.Equal("Scan not found", exception.ApiMessage);
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
