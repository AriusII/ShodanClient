namespace ShodanClient.Application.Configuration;

/// <summary>
///     Optional per-surface base-URL overrides. Any property left <see langword="null" /> falls back
///     to the built-in Shodan production host for that surface. Overriding is useful for testing
///     against a mock server or a proxy.
/// </summary>
public sealed class ShodanEndpointOptions
{
	/// <summary>Base URL for the core REST API (default <c>https://api.shodan.io/</c>).</summary>
	public Uri? RestBaseUrl { get; set; }

	/// <summary>Base URL for the Streaming API (default <c>https://stream.shodan.io/</c>).</summary>
	public Uri? StreamingBaseUrl { get; set; }

	/// <summary>Base URL for the Trends API (default <c>https://trends.shodan.io/</c>).</summary>
	public Uri? TrendsBaseUrl { get; set; }

	/// <summary>Base URL for the InternetDB API (default <c>https://internetdb.shodan.io/</c>).</summary>
	public Uri? InternetDbBaseUrl { get; set; }

	/// <summary>Base URL for the Exploits API (default <c>https://exploits.shodan.io/</c>).</summary>
	public Uri? ExploitsBaseUrl { get; set; }
}
