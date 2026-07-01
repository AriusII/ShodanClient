namespace ShodanClient.Infrastructure.Http;

/// <summary>
///     The distinct Shodan API surfaces. Each maps to its own base URL, authentication style and
///     <see cref="System.Net.Http.HttpClient" /> configuration (see <see cref="ShodanSurfaceRegistry" />).
/// </summary>
internal enum ShodanApiSurface
{
	/// <summary>Core REST API — <c>https://api.shodan.io</c>.</summary>
	Rest,

	/// <summary>Streaming API (NDJSON, long-lived) — <c>https://stream.shodan.io</c>.</summary>
	Streaming,

	/// <summary>Trends API (historical data) — <c>https://trends.shodan.io</c>.</summary>
	Trends,

	/// <summary>InternetDB API (key-less) — <c>https://internetdb.shodan.io</c>.</summary>
	InternetDb,

	/// <summary>Exploits API — <c>https://exploits.shodan.io</c>.</summary>
	Exploits
}
