using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.BulkData.Wire;

/// <summary>Wire shape of a single entry in the <c>GET /shodan/data</c> response array.</summary>
internal sealed class DatasetDto
{
	public string? Scope { get; set; }

	public string? Name { get; set; }

	public string? Description { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}
