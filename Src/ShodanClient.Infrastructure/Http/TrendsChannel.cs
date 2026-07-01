namespace ShodanClient.Infrastructure.Http;

/// <summary>Typed transport for the Trends API (<c>https://trends.shodan.io</c>).</summary>
internal sealed class TrendsChannel(HttpClient httpClient) : ShodanChannel(httpClient)
{
	protected override ShodanApiSurface Surface => ShodanApiSurface.Trends;
}
