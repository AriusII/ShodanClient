namespace ShodanClient.Infrastructure.Http;

/// <summary>Typed transport for the core REST API (<c>https://api.shodan.io</c>).</summary>
internal sealed class RestChannel(HttpClient httpClient) : ShodanChannel(httpClient)
{
	protected override ShodanApiSurface Surface => ShodanApiSurface.Rest;
}
