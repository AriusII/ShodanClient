using Microsoft.Extensions.DependencyInjection;
using ShodanClient.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace ShodanClient.Tests;

/// <summary>
///     Verifies the streaming surface reads newline-delimited JSON (one banner object per line) and
///     yields mapped domain banners via <see cref="IAsyncEnumerable{T}" />.
/// </summary>
public sealed class StreamIntegrationTests
{
	[Fact]
	public async Task Stream_StreamAllBannersAsync_yields_one_banner_per_ndjson_line()
	{
		using var server = WireMockServer.Start();
		const string ndjson = """
		                      {"ip_str":"1.1.1.1","port":80,"transport":"tcp"}
		                      {"ip_str":"2.2.2.2","port":443,"transport":"tcp"}
		                      """;
		server
			.Given(Request.Create().WithPath("/shodan/banners").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(200)
				.WithHeader("Content-Type", "text/plain").WithBody(ndjson));

		var services = new ServiceCollection();
		services.AddShodanClient(options =>
		{
			options.ApiKey = "test-key";
			options.Endpoints.StreamingBaseUrl = new Uri(server.Url!.EndsWith('/') ? server.Url : server.Url + "/");
		});
		await using var provider = services.BuildServiceProvider();
		var client = provider.GetRequiredService<IShodanClient>();

		var banners = new List<string>();
		await foreach (var banner in client.Stream.StreamAllBannersAsync(TestContext.Current.CancellationToken))
		{
			banners.Add(banner.IpString);
		}

		Assert.Equal(["1.1.1.1", "2.2.2.2"], banners);
	}
}
