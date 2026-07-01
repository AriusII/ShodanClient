using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.Scanning.Wire;

/// <summary>Wire shape of the <c>GET /shodan/scans/{id}</c> response.</summary>
internal sealed class ScanStatusResponse
{
	public string? Id { get; set; }

	public string? Status { get; set; }

	public string? Created { get; set; }

	public int Count { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}
