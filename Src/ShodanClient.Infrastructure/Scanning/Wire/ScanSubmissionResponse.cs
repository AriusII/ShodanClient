using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.Scanning.Wire;

/// <summary>Wire shape of the <c>POST /shodan/scan</c> response.</summary>
internal sealed class ScanSubmissionResponse
{
	public string? Id { get; set; }

	public int Count { get; set; }

	public int CreditsLeft { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}
