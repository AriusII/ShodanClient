using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.Directory.Wire;

/// <summary>Wire shape of the <c>GET /shodan/query/tags</c> response.</summary>
internal sealed class QueryTagsResponse
{
	public long Total { get; set; }

	public QueryTagDto[]? Matches { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}

/// <summary>Wire shape of a single popular-tag bucket (<c>{ count, value }</c>).</summary>
internal sealed class QueryTagDto
{
	public long Count { get; set; }

	[JsonPropertyName("value")] public string? Value { get; set; }
}
