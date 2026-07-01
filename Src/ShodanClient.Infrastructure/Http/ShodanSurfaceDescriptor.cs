namespace ShodanClient.Infrastructure.Http;

/// <summary>
///     Immutable description of a Shodan API surface: its default base address, whether it needs
///     the API key appended, whether it streams (long-lived), and the named <c>HttpClient</c> it uses.
/// </summary>
/// <param name="Surface">The surface this descriptor describes.</param>
/// <param name="DefaultBaseAddress">The production base address (always ends with <c>/</c>).</param>
/// <param name="RequiresApiKey">Whether requests must carry the <c>?key=</c> query parameter.</param>
/// <param name="IsStreaming">Whether the surface returns a long-lived streaming response.</param>
/// <param name="HttpClientName">The logical name of the typed <c>HttpClient</c> for this surface.</param>
internal readonly record struct ShodanSurfaceDescriptor(
	ShodanApiSurface Surface,
	Uri DefaultBaseAddress,
	bool RequiresApiKey,
	bool IsStreaming,
	string HttpClientName);
