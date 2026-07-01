using Microsoft.Extensions.DependencyInjection;
using ShodanClient.Application.Exceptions;
using ShodanClient.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace ShodanClient.Tests;

/// <summary>
///     End-to-end tests that exercise the whole vertical slice (DI → facade → service → repository →
///     channel → JSON source-gen → domain mapping) for the Notifiers surface against a stubbed HTTP
///     server.
/// </summary>
public sealed class NotifierIntegrationTests
{
	[Fact]
	public async Task Notifiers_ListAsync_maps_the_notifier_list()
	{
		using var server = WireMockServer.Start();
		const string json = """
		                    {
		                      "total": 2,
		                      "matches": [
		                        { "id": "abc123", "provider": "email", "description": "my email notifier", "args": { "to": "me@example.com" } },
		                        { "id": "def456", "provider": "slack", "description": null, "args": { "url": "https://hooks.slack.com/x" } }
		                      ]
		                    }
		                    """;
		server
			.Given(Request.Create().WithPath("/notifier").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var notifiers = await client.Notifiers.ListAsync(TestContext.Current.CancellationToken);

		Assert.Equal(2, notifiers.Count);
		Assert.Equal("abc123", notifiers[0].Id);
		Assert.Equal("email", notifiers[0].Provider);
		Assert.Equal("my email notifier", notifiers[0].Description);
		Assert.Equal("me@example.com", notifiers[0].Args["to"]);
		Assert.Equal("def456", notifiers[1].Id);
		Assert.Null(notifiers[1].Description);
		Assert.Equal("https://hooks.slack.com/x", notifiers[1].Args["url"]);
	}

	[Fact]
	public async Task Notifiers_GetProvidersAsync_maps_the_provider_map()
	{
		using var server = WireMockServer.Start();
		const string json = """
		                    {
		                      "email": { "required": ["to"] },
		                      "slack": { "required": ["url"] }
		                    }
		                    """;
		server
			.Given(Request.Create().WithPath("/notifier/provider").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var providers = await client.Notifiers.GetProvidersAsync(TestContext.Current.CancellationToken);

		Assert.Equal(2, providers.Count);
		var email = Assert.Single(providers, p => p.Name == "email");
		Assert.Equal(["to"], email.Required);
		var slack = Assert.Single(providers, p => p.Name == "slack");
		Assert.Equal(["url"], slack.Required);
	}

	[Fact]
	public async Task Notifiers_CreateAsync_posts_form_and_maps_the_result()
	{
		using var server = WireMockServer.Start();
		const string json = """{ "success": true, "id": "new-notifier-1" }""";
		server
			.Given(Request.Create().WithPath("/notifier").UsingPost())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var result = await client.Notifiers.CreateAsync(
			"email",
			"my new notifier",
			new Dictionary<string, string> { ["to"] = "me@example.com" },
			TestContext.Current.CancellationToken);

		Assert.True(result.Success);
		Assert.Equal("new-notifier-1", result.Id);

		var logEntry = Assert.Single(server.LogEntries);
		var body = logEntry.RequestMessage?.Body;
		Assert.NotNull(body);
		Assert.Contains("provider=email", body);
		Assert.Contains("description=my", body);
		Assert.Contains("to=me%40example.com", body);
	}

	[Fact]
	public async Task Notifiers_GetAsync_maps_a_single_notifier()
	{
		using var server = WireMockServer.Start();
		const string json = """
		                    { "id": "abc123", "provider": "webhook", "description": "hook", "args": { "url": "https://example.com/hook" } }
		                    """;
		server
			.Given(Request.Create().WithPath("/notifier/abc123").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody(json));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var notifier = await client.Notifiers.GetAsync("abc123", TestContext.Current.CancellationToken);

		Assert.Equal("abc123", notifier.Id);
		Assert.Equal("webhook", notifier.Provider);
		Assert.Equal("hook", notifier.Description);
		Assert.Equal("https://example.com/hook", notifier.Args["url"]);
	}

	[Fact]
	public async Task Notifiers_UpdateAsync_returns_true_on_success()
	{
		using var server = WireMockServer.Start();
		server
			.Given(Request.Create().WithPath("/notifier/abc123").UsingPut())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody("""{ "success": true }"""));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var updated = await client.Notifiers.UpdateAsync(
			"abc123",
			new Dictionary<string, string> { ["to"] = "someone-else@example.com" },
			TestContext.Current.CancellationToken);

		Assert.True(updated);
	}

	[Fact]
	public async Task Notifiers_DeleteAsync_returns_true_on_success()
	{
		using var server = WireMockServer.Start();
		server
			.Given(Request.Create().WithPath("/notifier/abc123").UsingDelete())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "application/json").WithBody("""{ "success": true }"""));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var deleted = await client.Notifiers.DeleteAsync("abc123", TestContext.Current.CancellationToken);

		Assert.True(deleted);
	}

	[Fact]
	public async Task Notifiers_GetAsync_throws_authentication_exception_on_401()
	{
		using var server = WireMockServer.Start();
		server
			.Given(Request.Create().WithPath("/notifier/abc123").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(401)
				.WithHeader("Content-Type", "application/json")
				.WithBody("""{ "error": "Invalid API key" }"""));

		await using var provider = BuildProvider(server.Url!);
		var client = provider.GetRequiredService<IShodanClient>();

		var exception = await Assert.ThrowsAsync<ShodanAuthenticationException>(() =>
			client.Notifiers.GetAsync("abc123", TestContext.Current.CancellationToken));

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
