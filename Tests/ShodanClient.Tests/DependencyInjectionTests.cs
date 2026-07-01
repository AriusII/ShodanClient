using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ShodanClient.Application.Configuration;
using ShodanClient.DependencyInjection;

namespace ShodanClient.Tests;

/// <summary>
///     Verifies the whole object graph is registered and resolvable — every sub-client of
///     <see cref="IShodanClient" /> must be constructible through DI.
/// </summary>
public sealed class DependencyInjectionTests
{
	[Fact]
	public void AddShodanClient_resolves_the_client_and_every_sub_client()
	{
		var services = new ServiceCollection();
		services.AddShodanClient(options => options.ApiKey = "test-key");

		using var provider = services.BuildServiceProvider(new ServiceProviderOptions
		{
			ValidateOnBuild = true,
			ValidateScopes = true
		});

		var client = provider.GetRequiredService<IShodanClient>();

		Assert.NotNull(client.Hosts);
		Assert.NotNull(client.Search);
		Assert.NotNull(client.Scans);
		Assert.NotNull(client.Alerts);
		Assert.NotNull(client.Notifiers);
		Assert.NotNull(client.Directory);
		Assert.NotNull(client.BulkData);
		Assert.NotNull(client.Organization);
		Assert.NotNull(client.Account);
		Assert.NotNull(client.Dns);
		Assert.NotNull(client.Tools);
		Assert.NotNull(client.ApiInfo);
		Assert.NotNull(client.InternetDb);
		Assert.NotNull(client.Trends);
		Assert.NotNull(client.Exploits);
		Assert.NotNull(client.Stream);
	}

	[Fact]
	public void Options_without_api_key_fail_validation()
	{
		var services = new ServiceCollection();
		services.AddShodanClient(options => options.ApiKey = string.Empty);

		using var provider = services.BuildServiceProvider();
		var options = provider.GetRequiredService<IOptions<ShodanClientOptions>>();

		// Materializing the options runs the source-generated validator, which rejects a blank key.
		Assert.Throws<OptionsValidationException>(() => _ = options.Value);
	}
}
