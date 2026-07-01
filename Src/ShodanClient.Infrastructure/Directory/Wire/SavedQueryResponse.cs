using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.Directory.Wire;

/// <summary>
///     Wire shape of the <c>GET /shodan/query</c> and <c>GET /shodan/query/search</c> responses.
/// </summary>
internal sealed class SavedQueryResponse
{
	public long Total { get; set; }

	public SavedQueryDto[]? Matches { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}

/// <summary>Wire shape of a single saved-query entry.</summary>
internal sealed class SavedQueryDto
{
	public int Votes { get; set; }

	public string? Description { get; set; }

	public string? Title { get; set; }

	public string? Timestamp { get; set; }

	public string[]? Tags { get; set; }

	public string? Query { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}
