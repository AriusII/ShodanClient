using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.Scanning.Wire;

/// <summary>Wire shape of the <c>GET /shodan/scans</c> response.</summary>
internal sealed class ScanListResponse
{
	public long Total { get; set; }

	public ScanListEntryDto[]? Matches { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}

/// <summary>Wire shape of a single entry in the <c>matches</c> array of a scan list.</summary>
internal sealed class ScanListEntryDto
{
	public string? Id { get; set; }

	public string? Created { get; set; }

	public string? Status { get; set; }

	public int Size { get; set; }

	public int CreditsLeft { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}
