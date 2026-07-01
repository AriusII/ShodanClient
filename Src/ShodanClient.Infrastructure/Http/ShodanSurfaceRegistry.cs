using System.Collections.Frozen;

namespace ShodanClient.Infrastructure.Http;

/// <summary>
///     The single source of truth mapping each <see cref="ShodanApiSurface" /> to its base URL,
///     authentication requirement, streaming flag and typed-client name. Base URLs live here and
///     nowhere else; routes are always relative and never carry the host or the API key.
/// </summary>
internal static class ShodanSurfaceRegistry
{
	/// <summary>All surface descriptors, keyed by surface.</summary>
	public static readonly FrozenDictionary<ShodanApiSurface, ShodanSurfaceDescriptor> All =
		new Dictionary<ShodanApiSurface, ShodanSurfaceDescriptor>
		{
			[ShodanApiSurface.Rest] = new(
				ShodanApiSurface.Rest, new Uri("https://api.shodan.io/"),
				true, false, "shodan-rest"),

			[ShodanApiSurface.Streaming] = new(
				ShodanApiSurface.Streaming, new Uri("https://stream.shodan.io/"),
				true, true, "shodan-stream"),

			[ShodanApiSurface.Trends] = new(
				ShodanApiSurface.Trends, new Uri("https://trends.shodan.io/"),
				true, false, "shodan-trends"),

			[ShodanApiSurface.InternetDb] = new(
				ShodanApiSurface.InternetDb, new Uri("https://internetdb.shodan.io/"),
				false, false, "shodan-internetdb"),

			[ShodanApiSurface.Exploits] = new(
				ShodanApiSurface.Exploits, new Uri("https://exploits.shodan.io/"),
				true, false, "shodan-exploits")
		}.ToFrozenDictionary();

	/// <summary>Gets the descriptor for a surface.</summary>
	public static ShodanSurfaceDescriptor For(ShodanApiSurface surface) => All[surface];
}
