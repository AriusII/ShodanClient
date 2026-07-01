using Microsoft.Extensions.DependencyInjection;
using ShodanClient.Application.Exceptions;
using ShodanClient.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace ShodanClient.Tests;

/// <summary>
///     End-to-end tests that exercise the whole vertical slice (DI → facade → service → repository →
///     channel → JSON source-gen → domain mapping) against a stubbed HTTP server, for the small
///     Account, API status and Utility bounded contexts.
/// </summary>
public sealed class AccountApiStatusUtilityIntegrationTests
{
	#region ApiStatus

	[Fact]
	public async Task ApiInfo_GetAsync_maps_plan_credits_and_usage_limits()
	{
		using var server = WireMockServer.Start();
		const string json = """
		                    {
		                      "scan_credits": 100000,
		                      "usage_limits": {
		                        "scan_credits": -1,
		                        "query_credits": -1,
		                        "monitored_ips": -1
		                      },
		                      "plan": "stream-100",
		                      "https": false,
		                      "unlocked": true,
		                      "query_credits": 100000,
		                      "monitored_ips": 19,
		                      "unlocked_left": 100000,
		                      "telnet": false
		                    }
		                    """;
		server
			.Given(Request.Create().WithPath("/api-info").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var info = await client.ApiInfo.GetAsync(TestContext.Current.CancellationToken);

		Assert.Equal(100000, info.ScanCredits);
		Assert.Equal(100000, info.QueryCredits);
		Assert.Equal(19, info.MonitoredIps);
		Assert.Equal("stream-100", info.Plan);
		Assert.False(info.Https);
		Assert.False(info.Telnet);
		Assert.True(info.Unlocked);
		Assert.Equal(100000, info.UnlockedLeft);
		Assert.Equal(-1, info.UsageLimits.ScanCredits);
		Assert.Equal(-1, info.UsageLimits.QueryCredits);
		Assert.Equal(-1, info.UsageLimits.MonitoredIps);
	}

	#endregion

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

	#region Account

	[Fact]
	public async Task Account_GetProfileAsync_maps_membership_credits_and_created()
	{
		using var server = WireMockServer.Start();
		const string json = """
		                    {
		                      "member": true,
		                      "credits": 0,
		                      "display_name": null,
		                      "created": "2020-06-15T10:44:43.148000"
		                    }
		                    """;
		server
			.Given(Request.Create().WithPath("/account/profile").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var profile = await client.Account.GetProfileAsync(TestContext.Current.CancellationToken);

		Assert.True(profile.Member);
		Assert.Equal(0, profile.Credits);
		Assert.Null(profile.DisplayName);
		Assert.NotNull(profile.Created);
		Assert.Equal(new DateOnly(2020, 6, 15), DateOnly.FromDateTime(profile.Created!.Value.UtcDateTime));
	}

	[Fact]
	public async Task Account_GetProfileAsync_throws_authentication_exception_on_401()
	{
		using var server = WireMockServer.Start();
		server
			.Given(Request.Create().WithPath("/account/profile").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(401)
				.WithHeader("Content-Type", "application/json")
				.WithBody("""{ "error": "Invalid API key" }"""));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var exception = await Assert.ThrowsAsync<ShodanAuthenticationException>(() =>
			client.Account.GetProfileAsync(TestContext.Current.CancellationToken));

		Assert.Equal("Invalid API key", exception.ApiMessage);
	}

	#endregion

	#region Utility

	[Fact]
	public async Task Tools_GetHttpHeadersAsync_maps_the_header_dictionary()
	{
		using var server = WireMockServer.Start();
		const string json = """
		                    {
		                      "Content-Length": "",
		                      "Accept-Encoding": "gzip",
		                      "X-Forwarded-For": "113.161.57.41",
		                      "Host": "api.shodan.io",
		                      "User-Agent": "curl/7.64.1",
		                      "Connection": "Keep-Alive",
		                      "X-Forwarded-Proto": "https",
		                      "Accept": "*/*",
		                      "Content-Type": ""
		                    }
		                    """;
		server
			.Given(Request.Create().WithPath("/tools/httpheaders").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var headers = await client.Tools.GetHttpHeadersAsync(TestContext.Current.CancellationToken);

		Assert.Equal("curl/7.64.1", headers["User-Agent"]);
		Assert.Equal("api.shodan.io", headers["Host"]);
		Assert.Equal("gzip", headers["Accept-Encoding"]);
	}

	[Fact]
	public async Task Tools_GetMyIpAsync_maps_the_ip_string()
	{
		using var server = WireMockServer.Start();
		server
			.Given(Request.Create().WithPath("/tools/myip").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody("\"113.161.57.41\""));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var ip = await client.Tools.GetMyIpAsync(TestContext.Current.CancellationToken);

		Assert.Equal("113.161.57.41", ip);
	}

	[Fact]
	public async Task Tools_GetMyIpAsync_throws_not_found_on_404()
	{
		using var server = WireMockServer.Start();
		server
			.Given(Request.Create().WithPath("/tools/myip").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(404)
				.WithHeader("Content-Type", "application/json")
				.WithBody("""{ "error": "Not found" }"""));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var exception = await Assert.ThrowsAsync<ShodanNotFoundException>(() =>
			client.Tools.GetMyIpAsync(TestContext.Current.CancellationToken));

		Assert.Equal("Not found", exception.ApiMessage);
	}

	#endregion
}
