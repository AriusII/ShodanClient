using Microsoft.Extensions.DependencyInjection;
using ShodanClient.Application.Exceptions;
using ShodanClient.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace ShodanClient.Tests;

/// <summary>
///     End-to-end tests that exercise the whole vertical slice (DI → facade → service → repository →
///     channel → JSON source-gen → domain mapping) for the DNS surface against a stubbed HTTP server.
/// </summary>
public sealed class DnsIntegrationTests
{
	[Fact]
	public async Task Dns_GetDomainAsync_sends_history_type_and_page_and_maps_the_records()
	{
		using var server = WireMockServer.Start();
		const string json = """
		                    {
		                      "domain": "example.com",
		                      "tags": ["compromised"],
		                      "subdomains": ["www", "mail"],
		                      "more": true,
		                      "data": [
		                        {
		                          "subdomain": "www",
		                          "type": "A",
		                          "value": "93.184.216.34",
		                          "last_seen": "2024-09-29T09:39:45.813661",
		                          "ports": [80, 443]
		                        },
		                        {
		                          "subdomain": "",
		                          "type": "MX",
		                          "value": "mail.example.com",
		                          "last_seen": "2024-08-01T00:00:00.000000"
		                        }
		                      ]
		                    }
		                    """;
		server
			.Given(Request.Create().WithPath("/dns/domain/example.com")
				.WithParam("history", "true")
				.WithParam("type", "A")
				.WithParam("page", "2")
				.UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var info = await client.Dns.GetDomainAsync("example.com", true, "A", 2,
			TestContext.Current.CancellationToken);

		Assert.Equal("example.com", info.Domain);
		Assert.Equal(["compromised"], info.Tags);
		Assert.Equal(["www", "mail"], info.Subdomains);
		Assert.True(info.More);
		Assert.Equal(2, info.Data.Count);
		Assert.Equal("www", info.Data[0].Subdomain);
		Assert.Equal("A", info.Data[0].Type);
		Assert.Equal("93.184.216.34", info.Data[0].Value);
		Assert.Equal([80, 443], info.Data[0].Ports);
		Assert.NotNull(info.Data[0].LastSeen);
		Assert.Equal(string.Empty, info.Data[1].Subdomain);
		Assert.Equal("MX", info.Data[1].Type);
		Assert.Empty(info.Data[1].Ports);
	}

	[Fact]
	public async Task Dns_ResolveAsync_maps_hostnames_to_ips_and_null_for_unresolved()
	{
		using var server = WireMockServer.Start();
		const string json = """
		                    {
		                      "example.com": "93.184.216.34",
		                      "doesnotexist.example": null
		                    }
		                    """;
		server
			.Given(Request.Create().WithPath("/dns/resolve")
				.WithParam("hostnames", "example.com,doesnotexist.example")
				.UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var result = await client.Dns.ResolveAsync(["example.com", "doesnotexist.example"],
			TestContext.Current.CancellationToken);

		Assert.Equal(2, result.Count);
		Assert.Equal("93.184.216.34", result["example.com"]);
		Assert.Null(result["doesnotexist.example"]);
	}

	[Fact]
	public async Task Dns_ReverseAsync_maps_ips_to_hostnames_and_empty_list_for_no_ptr()
	{
		using var server = WireMockServer.Start();
		const string json = """
		                    {
		                      "8.8.8.8": ["dns.google"],
		                      "1.2.3.4": null
		                    }
		                    """;
		server
			.Given(Request.Create().WithPath("/dns/reverse")
				.WithParam("ips", "8.8.8.8,1.2.3.4")
				.UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var result = await client.Dns.ReverseAsync(["8.8.8.8", "1.2.3.4"],
			TestContext.Current.CancellationToken);

		Assert.Equal(2, result.Count);
		Assert.Equal(["dns.google"], result["8.8.8.8"]);
		Assert.Empty(result["1.2.3.4"]);
	}

	[Fact]
	public async Task Dns_GetDomainAsync_throws_not_found_on_404()
	{
		using var server = WireMockServer.Start();
		server
			.Given(Request.Create().WithPath("/dns/domain/doesnotexist.example").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(404)
				.WithHeader("Content-Type", "application/json")
				.WithBody("""{ "error": "No DNS entries found for domain: doesnotexist.example" }"""));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var exception = await Assert.ThrowsAsync<ShodanNotFoundException>(() =>
			client.Dns.GetDomainAsync("doesnotexist.example",
				cancellationToken: TestContext.Current.CancellationToken));

		Assert.Equal("No DNS entries found for domain: doesnotexist.example", exception.ApiMessage);
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
