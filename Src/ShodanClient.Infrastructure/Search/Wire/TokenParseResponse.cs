using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.Search.Wire;

/// <summary>Wire shape of the <c>GET /shodan/host/search/tokens</c> response.</summary>
internal sealed class TokenParseResponse
{
	// The free-text portion of the query is returned under the "string" key.
	[JsonPropertyName("string")] public string? SearchTerm { get; set; }

	public Dictionary<string, JsonElement>? Attributes { get; set; }

	public string[]? Filters { get; set; }

	public string[]? Errors { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}
