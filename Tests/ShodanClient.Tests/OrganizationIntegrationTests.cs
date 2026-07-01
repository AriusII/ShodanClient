using Microsoft.Extensions.DependencyInjection;
using ShodanClient.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace ShodanClient.Tests;

/// <summary>
///     Regression coverage for the Organization surface: <c>GET /org</c> returns <c>admins</c>/
///     <c>members</c> as arrays of <c>{ username, email }</c> objects (not a flat <c>string[]</c>) and
///     <c>logo</c> as a boolean (not a URL string). The original wire DTO modeled the wrong shape for
///     both, which made deserialization throw for any organization with admins/members populated.
/// </summary>
public sealed class OrganizationIntegrationTests
{
	[Fact]
	public async Task Organization_GetAsync_maps_admins_members_and_logo()
	{
		using var server = WireMockServer.Start();
		const string json = """
		                    {
		                      "name": "Shodan Organization",
		                      "created": "2020-09-30T15:41:48.073000",
		                      "admins": [ { "username": "admin", "email": "admin@shodan.io" } ],
		                      "members": [ { "username": "member", "email": "member@shodan.io" } ],
		                      "upgrade_type": "stream-100",
		                      "domains": [ "shodan.io" ],
		                      "logo": false,
		                      "id": "abc123"
		                    }
		                    """;
		server
			.Given(Request.Create().WithPath("/org").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var organization = await client.Organization.GetAsync(TestContext.Current.CancellationToken);

		Assert.Equal("Shodan Organization", organization.Name);
		var admin = Assert.Single(organization.Admins);
		Assert.Equal("admin", admin.Username);
		Assert.Equal("admin@shodan.io", admin.Email);
		var member = Assert.Single(organization.Members);
		Assert.Equal("member", member.Username);
		Assert.Equal("stream-100", organization.UpgradeType);
		Assert.False(organization.Logo);
		Assert.Contains("shodan.io", organization.Domains);
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
