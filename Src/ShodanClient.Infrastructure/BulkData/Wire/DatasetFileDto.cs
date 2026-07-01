using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.BulkData.Wire;

/// <summary>Wire shape of a single entry in the <c>GET /shodan/data/{dataset}</c> response array.</summary>
internal sealed class DatasetFileDto
{
	public string? Name { get; set; }

	public long Size { get; set; }

	public string? Url { get; set; }

	public string? Sha1 { get; set; }

	public string? Timestamp { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}
