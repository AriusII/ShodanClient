namespace ShodanClient.Infrastructure.Http;

/// <summary>
///     Typed transport for the InternetDB API (<c>https://internetdb.shodan.io</c>). This surface is
///     key-less, so the API-key handler is deliberately NOT attached to its client.
/// </summary>
internal sealed class InternetDbChannel(HttpClient httpClient) : ShodanChannel(httpClient)
{
	protected override ShodanApiSurface Surface => ShodanApiSurface.InternetDb;
}
