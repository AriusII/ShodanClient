namespace ShodanClient.Infrastructure.Http;

/// <summary>
///     Typed transport for the Streaming API (<c>https://stream.shodan.io</c>). Its client uses an
///     infinite timeout and no aggressive resilience so long-lived NDJSON connections are never aborted.
/// </summary>
internal sealed class StreamingChannel(HttpClient httpClient) : ShodanChannel(httpClient)
{
	protected override ShodanApiSurface Surface => ShodanApiSurface.Streaming;
}
