using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.Scanning.Wire;

/// <summary>Wire shape of the <c>POST /shodan/scan/internet</c> response.</summary>
internal sealed class ScanInternetResponse
{
	public string? Id { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}
