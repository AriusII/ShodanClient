using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.InternetDb.Wire;

/// <summary>Wire shape of the <c>GET https://internetdb.shodan.io/{ip}</c> response.</summary>
internal sealed class InternetDbResponse
{
	public string? Ip { get; set; }

	public int[]? Ports { get; set; }

	public string[]? Cpes { get; set; }

	public string[]? Hostnames { get; set; }

	public string[]? Tags { get; set; }

	public string[]? Vulns { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}
